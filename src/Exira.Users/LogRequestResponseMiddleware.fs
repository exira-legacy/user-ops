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
                async {
                    resetStream stream
                    use reader =
                        new StreamReader(
                            stream,
                            encoding = Encoding.UTF8,
                            detectEncodingFromByteOrderMarks = true,
                            bufferSize = 1024,
                            leaveOpen = true)
                    let! b = reader.ReadToEndAsync() |> Async.AwaitTask
                    resetStream stream
                    return b
                }

            let copyBuffer (buffer: Stream) stream =
                async {
                    resetStream buffer
                    return! buffer.CopyToAsync stream |> Async.AwaitTask
                }

            let logRequest (request: IOwinRequest) =
                async {
                    match options.Logger with
                    | Some logger ->
                        let! requestBody = readStream request.Body

                        let l =
                            logger
                                .ForContext("Accept", request.Accept)
                                .ForContext("Body", requestBody)
                                .ForContext("CacheControl", request.CacheControl)
                                .ForContext("ContentType", request.ContentType)
                                .ForContext("Cookies", request.Cookies, true)
                                .ForContext("Headers", request.Headers, true)
                                .ForContext("Host", request.Host)
                                .ForContext("MediaType", request.MediaType)
                                .ForContext("Path", request.Path)
                                .ForContext("Protocol", request.Protocol)
                                .ForContext("QueryString", request.QueryString)
                                .ForContext("Scheme", request.Scheme)
                                .ForContext("User", request.User)

                        l.Debug("Incoming request: {Method:l} {Uri:l}", request.Method, (request.Uri.ToString()))
                    | None -> printfn "Incoming request: %s %s" request.Method (request.Uri.ToString())
                }

            let logResponse (response: IOwinResponse) =
                async {
                    match options.Logger with
                    | Some logger ->
                        let! responseBody = readStream response.Body

                        let l =
                            logger
                                .ForContext("Body", responseBody)
                                .ForContext("ContentLength", response.ContentLength)
                                .ForContext("ContentType", response.ContentType)
                                .ForContext("Cookies", response.Cookies, true)
                                .ForContext("ETag", response.ETag)
                                .ForContext("Expires", response.Expires)
                                .ForContext("Headers", response.Headers, true)
                                .ForContext("Protocol", response.Protocol)

                        l.Debug("Outgoing response: {StatusCode} {ReasonPhrase:l}", response.StatusCode, response.ReasonPhrase)
                    | None -> printfn "Outgoing response: %i %s" response.StatusCode response.ReasonPhrase
                }

            async {
                let responseStream = response.Body
                use responseBuffer = new MemoryStream()
                response.Body <- responseBuffer

                do! logRequest request
                do! next.Invoke environment |> awaitTask
                do! logResponse response

                do! copyBuffer responseBuffer responseStream
            } |> Async.StartAsTask :> Task

    [<ExtensionAttribute>]
    type internal AppBuilderExtensions =
        [<ExtensionAttribute>]
        static member UseLogRequestResponse(appBuilder: IAppBuilder, options: LogRequestResponseOptions) =
            appBuilder.Use<LogRequestResponseMiddleware>(options)