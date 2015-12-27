namespace Exira.Users.Web

open System
open System.IO
open System.Net
open System.Web.Hosting

open Owin
open Microsoft.Owin
open Microsoft.Owin.BuilderProperties

open Akka.Actor

[<Sealed>]
type Startup() =
    let configuration = Settings.configuration

    let mutable (actorSystem: ActorSystem option) = None

    let registerActorSystem (app: IAppBuilder) =
        let startActorSystem () =
            actorSystem <- bootstrapActorSystem()

        let stopActorSystem () =
            match actorSystem with
                | None -> ()
                | Some system ->
                    system.Shutdown()
                    system.AwaitTermination()
                    actorSystem <- None

        let properties = AppProperties app.Properties
        let token = properties.OnAppDisposing
        token.Register (fun _ -> stopActorSystem()) |> ignore
        startActorSystem()

    let registerSignalR (app: IAppBuilder) =
        app.MapSignalR() |> ignore

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
            .Map("/changepassword", fun inner -> servePage inner configuration.Pages.ChangePasswordPage)
            .Map("/verify", fun inner -> servePage inner configuration.Pages.VerifyPage)
            .Map("/verifypasswordreset", fun inner -> servePage inner configuration.Pages.VerifyPasswordResetPage)
            |> ignore

        servePage app configuration.Pages.LoginPage

    member __.Configuration (app: IAppBuilder) =
        registerActorSystem app
        registerSignalR app
        registerPages app

