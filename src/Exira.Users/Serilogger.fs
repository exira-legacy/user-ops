namespace Exira.Users

module internal Serilogger =
    open Serilog

    let private loggerConfig = Configuration.webConfig.Logging
    let [<Literal>] private OutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{RequestId}] [{Level}] {Message}{NewLine}{Exception}"

    let private properties (logger: LoggerConfiguration) =
        loggerConfig.Properties
        |> Seq.fold (fun (logger: LoggerConfiguration) p -> logger.Enrich.WithProperty(p.key, p.value)) logger

    let private console (logger: LoggerConfiguration) =
        if loggerConfig.Sinks.Console.Enabled then
            logger.WriteTo.ColoredConsole(
                outputTemplate = OutputTemplate)
        else logger

    let private rollingFile (logger: LoggerConfiguration) =
        if loggerConfig.Sinks.RollingFile.Enabled then
            logger.WriteTo.RollingFile(
                loggerConfig.Sinks.RollingFile.PathFormat,
                outputTemplate = OutputTemplate)
        else logger

    let private seq (logger: LoggerConfiguration) =
        if loggerConfig.Sinks.Seq.Enabled then
            logger.WriteTo.Seq(
                loggerConfig.Sinks.Seq.Url.ToString(),
                apiKey = loggerConfig.Sinks.Seq.ApiKey)
        else logger

    let logger =
        let logger =
            LoggerConfiguration()
                .MinimumLevel.Debug()
                .Destructure.FSharpTypes()
                .Enrich.FromLogContext()
            |> properties
            |> console
            |> rollingFile
            |> seq

        logger.CreateLogger()