namespace Exira.Users

[<AutoOpen>]
module internal Serilogger =
    open Serilog

    let private loggerConfig = Configuration.webConfig.Logging

    let logger =
        let logger = LoggerConfiguration()

        let logger =
            loggerConfig.Properties
            |> Seq.fold (fun (logger: LoggerConfiguration) p -> logger.Enrich.WithProperty(p.key, p.value)) logger

        let logger =
            if loggerConfig.Sinks.Console.Enabled then
                logger.WriteTo.ColoredConsole()
            else logger

        let logger =
            if loggerConfig.Sinks.RollingFile.Enabled then
                logger.WriteTo.RollingFile(loggerConfig.Sinks.RollingFile.PathFormat)
            else logger

        let logger =
            if loggerConfig.Sinks.Seq.Enabled then
                logger.WriteTo.Seq(loggerConfig.Sinks.Seq.Url.ToString(), apiKey = loggerConfig.Sinks.Seq.ApiKey)
            else logger

        logger.CreateLogger()