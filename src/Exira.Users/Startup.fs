namespace Exira.Users

open Owin
open Microsoft.Owin
open Microsoft.Owin.Extensions
open Microsoft.Owin.Security
open Microsoft.Owin.Security.Cookies

open System
open System.IO
open System.Net
open System.Web.Hosting
open System.Web.Http
open System.Web.Http.Cors

open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Exira.EventStore.Owin
open Exira.VersionHeader.Owin

open Exira.CsrfValidation.Owin
open Converters

[<Sealed>]
type Startup() =
    let webConfig = Configuration.webConfig
    let logger = Serilogger.logger

    let registerVersionHeader (app: IAppBuilder) =
        let config =
            VersionHeaderOptions(
                versionType = typedefof<Startup>,
                HeaderName = webConfig.Debug.Web.ReleaseVersionHeaderName,
                HeaderFormat = webConfig.Debug.Web.ReleaseVersionHeaderFormat)

        if webConfig.Debug.Web.Enabled
        then app.UseVersionHeader config |> ignore

    let registerCookieAuthentication (app: IAppBuilder) =
        let cookieOptions =
            CookieAuthenticationOptions(
                AuthenticationMode = AuthenticationMode.Active,
                CookieHttpOnly = true,
                CookieSecure = CookieSecureOption.SameAsRequest,
                SlidingExpiration = true,
                AuthenticationType = webConfig.Web.Authentication.AuthenticationName,
                CookieName = webConfig.Web.Authentication.CookieName,
                CookiePath = webConfig.Web.Authentication.CookiePath)

        if webConfig.Debug.Authentication.UseCookieDomain then
            cookieOptions.CookieDomain <- webConfig.Web.Authentication.CookieDomain

        let csrfOptions =
            CsrfValidationOptions(
                ExcludedPaths =
                    [
                        "/users/"
                        "/users/.*/login"
                        "/users/.*/verify"
                        "/users/.*/requestpasswordreset"
                        "/users/.*/verifypasswordreset"
                    ])

        app.UseCookieAuthentication cookieOptions |> ignore
        app.UseCsrfValidation csrfOptions |> ignore

    let configureRouting (config: HttpConfiguration) =
        config.MapHttpAttributeRoutes()
        config

    let configureFormatters (config: HttpConfiguration)  =
        config.Formatters.Remove config.Formatters.XmlFormatter |> ignore
        config.Formatters.JsonFormatter.SerializerSettings.ContractResolver <- CamelCasePropertyNamesContractResolver()
        config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling <- ReferenceLoopHandling.Ignore
        config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling <- NullValueHandling.Ignore
        config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new OptionConverter())
        config

    let configureCors (config: HttpConfiguration) =
        let urls =
            webConfig.Web.CORS.AllowedOrigins
            |> Seq.map (fun uri -> uri.ToString().TrimEnd('/'))
            |> String.concat ","

        let cors = EnableCorsAttribute(urls, "*", "*")
        config.EnableCors cors
        config

    let configureApi (inner: IAppBuilder) (config: HttpConfiguration) =
        registerCookieAuthentication inner

        inner.UseWebApi config |> ignore
        inner.UseStageMarker PipelineStage.MapHandler |> ignore

    let registerWebApi (app: IAppBuilder) (basePath: string) =
        let config =
            new HttpConfiguration()
            |> configureRouting
            |> configureFormatters
            |> configureCors

        app.Map(basePath, fun inner -> configureApi inner config) |> ignore

    let registerEventStore (app: IAppBuilder) =
        let config =
            EventStoreOptions(ConnectionString = webConfig.EventStore.ConnectionString)

        app.UseEventStore config |> ignore

    let renderPage (context: IOwinContext) path =
        try
            let page =
                path
                |> HostingEnvironment.MapPath
                |> File.ReadAllText

            context.Response.ContentType <- "text/html"
            context.Response.StatusCode <- int HttpStatusCode.OK
            context.Response.ContentLength <- Nullable<int64>(int64 page.Length)

            context.Response.WriteAsync page
        with
        | ex -> context.Response.WriteAsync ex.Message

    let registerPages (app: IAppBuilder) =
        let servePage (appBuilder: IAppBuilder) page =
            appBuilder.Run (fun c -> renderPage c page)

        app
            .Map("/changepassword", fun inner -> servePage inner webConfig.Web.Pages.ChangePasswordPage)
            .Map("/verify", fun inner -> servePage inner webConfig.Web.Pages.VerifyPage)
            .Map("/verifypasswordreset", fun inner -> servePage inner webConfig.Web.Pages.VerifyPasswordResetPage)
            |> ignore

        servePage app webConfig.Web.Pages.LoginPage

    member __.Configuration(app: IAppBuilder) =
        logger.Information "Starting user-ops"
        registerVersionHeader app
        registerEventStore app
        registerWebApi app "/api"
        registerPages app
        logger.Information "Started user-ops"

