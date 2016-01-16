namespace Exira.Serilog.Owin

[<AutoOpen>]
module SerilogRequestContext =
    open Owin
    open System
    open System.Collections.Generic
    open System.Threading.Tasks
    open System.Runtime.CompilerServices
    open Microsoft.Owin
    open Serilog.Context
    open Serilog.Core
    open Serilog.Events

    type internal RequestIdEnricher(context: OwinContext) =
        let [<Literal>] OwinRequestId = "owin.RequestId"

        interface ILogEventEnricher with
            member this.Enrich(logEvent, _) =
                let correlationId = context.Get<string>(OwinRequestId)

                let correlationId =
                    if correlationId = String.Empty then Guid.NewGuid().ToString("D")
                    else correlationId

                let property = LogEventProperty("RequestId", ScalarValue correlationId)
                logEvent.AddPropertyIfAbsent property

    type internal RequestUriEnricher(context: OwinContext) =
        interface ILogEventEnricher with
            member this.Enrich(logEvent, _) =
                let property = LogEventProperty("RequestUri", ScalarValue context.Request.Uri)
                logEvent.AddPropertyIfAbsent property

    type internal RemoteIpAddressEnricher(context: OwinContext) =
        interface ILogEventEnricher with
            member this.Enrich(logEvent, _) =
                let property = LogEventProperty("RemoteIp", ScalarValue context.Request.RemoteIpAddress)
                logEvent.AddPropertyIfAbsent property

    type SerilogRequestContextMiddleware(next: Func<IDictionary<string, obj>, Task>) =
        let awaitTask = Async.AwaitIAsyncResult >> Async.Ignore

        member this.Invoke (environment: IDictionary<string, obj>) =
            let context = OwinContext environment

            use properties =
                LogContext.PushProperties(
                    RequestIdEnricher(context),
                    RequestUriEnricher(context),
                    RemoteIpAddressEnricher(context))

            async {
                do! next.Invoke environment |> awaitTask
            } |> Async.StartAsTask :> Task

    [<ExtensionAttribute>]
    type internal AppBuilderExtensions =
        [<ExtensionAttribute>]
        static member UseSerilogRequestContext(appBuilder: IAppBuilder) =
            appBuilder.Use<SerilogRequestContextMiddleware>()