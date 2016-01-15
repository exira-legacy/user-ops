namespace Exira.Serilog.Owin

open Owin
open System
open System.Collections.Generic
open System.Threading.Tasks
open System.Runtime.CompilerServices
open Microsoft.Owin
open Serilog.Context

// TODO: CorrelationId and other info for logger should come from owin or something
// using (LogContext.PushProperties(new PropertyEnricher("Request.Uri", context.Request.Uri.AbsoluteUri)))
//public override async Task Invoke(IOwinContext context)
//{
//    using (LogContext.PushProperties(
//        RequestUri(context),
//        RemoteIp(context)))
//        {
//            await Next.Invoke(context);
//        }
//    }

type SerilogRequestContextMiddleware(next: Func<IDictionary<string, obj>, Task>) =
    let awaitTask = Async.AwaitIAsyncResult >> Async.Ignore

    let [<Literal>] OwinRequestId = "owin.RequestId"

    let add key value (dict: dict<'Key, 'T>) =
        lock dict <| fun () ->
            if (dict.ContainsKey(key)) then ()
            else dict.[key] <- value
        dict

    member this.Invoke (environment: IDictionary<string, obj>) =
        let context = OwinContext environment

        let correlationId = context.Get<Guid>(OwinRequestId)
        let correlationId =
            if correlationId = Guid.Empty then Guid.NewGuid()
            else correlationId

        let updatedEnvironment = environment |> add OwinRequestId (correlationId :> obj)

        use l = LogContext.PushProperty("CorrelationId", correlationId)

        async {
            do! next.Invoke updatedEnvironment |> awaitTask
        } |> Async.StartAsTask :> Task

[<ExtensionAttribute>]
type AppBuilderExtensions =
    [<ExtensionAttribute>]
    static member UseSerilogRequestContext(appBuilder: IAppBuilder) =
        appBuilder.Use<SerilogRequestContextMiddleware>()