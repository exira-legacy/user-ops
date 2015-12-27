namespace Exira.Users.Web

open Owin
open Microsoft.Owin

open System
open System.IO
open System.Net
open System.Web.Hosting

[<Sealed>]
type Startup() =
    let webConfig = Configuration.webConfig

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
            .Map("/changepassword", fun inner -> servePage inner webConfig.Pages.ChangePasswordPage)
            .Map("/verify", fun inner -> servePage inner webConfig.Pages.VerifyPage)
            .Map("/verifypasswordreset", fun inner -> servePage inner webConfig.Pages.VerifyPasswordResetPage)
            |> ignore

        servePage app webConfig.Pages.LoginPage

    member __.Configuration(app: IAppBuilder) =
        registerPages app

