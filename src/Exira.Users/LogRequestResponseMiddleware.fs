namespace Exira.Serilog.Owin

[<AutoOpen>]
module LogRequestResponse =
    open Owin
    open System
    open System.IO
    open System.Text
    open System.Threading.Tasks
    open System.Collections.Generic
    open System.Runtime.CompilerServices
    open Microsoft.Owin
    open Serilog

    type LogRequestResponseOptions() =
        member val Logger: ILogger option = None with get, set

    type LogRequestResponseMiddleware(next: Func<IDictionary<string, obj>, Task>, options: LogRequestResponseOptions) =
        let awaitTask = Async.AwaitIAsyncResult >> Async.Ignore

        member this.Invoke (environment: IDictionary<string, obj>) =
            let context = OwinContext environment
            let request = context.Request
            let response = context.Response

            let resetStream (stream: Stream) =
                if stream.CanSeek then
                    stream.Seek(int64 0, SeekOrigin.Begin) |> ignore
                else ()

            let readStream (stream: Stream) =
                resetStream stream
                use reader =
                    new StreamReader(
                        stream,
                        encoding = Encoding.UTF8,
                        detectEncodingFromByteOrderMarks = true,
                        bufferSize = 1024,
                        leaveOpen = true)
                let b = reader.ReadToEndAsync() // TODO: ReadToEndAsync?
                resetStream stream
                b

            let copyBuffer (buffer: Stream) stream =
                resetStream buffer
                buffer.CopyTo stream // TODO: CopyToAsync?

            let log prefix (message: string) =
                match options.Logger with
                | Some logger -> logger.Debug(sprintf "%s: {request}" prefix, message)
                | None -> printfn "%s: %s" prefix message

            async {
                let responseStream = response.Body
                use responseBuffer = new MemoryStream()
                response.Body <- responseBuffer

                let requestBody = readStream request.Body
                log "Incoming request" requestBody

                do! next.Invoke environment |> awaitTask

                let response = readStream responseBuffer
                copyBuffer responseBuffer responseStream
                log "Outgoing response" response
            } |> Async.StartAsTask :> Task

    [<ExtensionAttribute>]
    type internal AppBuilderExtensions =
        [<ExtensionAttribute>]
        static member UseLogRequestResponse(appBuilder: IAppBuilder, options: LogRequestResponseOptions) =
            appBuilder.Use<LogRequestResponseMiddleware>(options)