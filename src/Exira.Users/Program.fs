open System.Reflection
open System.IO
open FSharp.Configuration
open Topshelf
open Time

let executablePath = Assembly.GetEntryAssembly().Location |> Path.GetDirectoryName
let configPath = Path.Combine(executablePath, "Configuration.yaml")

type Configuration = YamlConfig<"Configuration.yaml">
let configuration = Configuration()
configuration.Load configPath

let stop _ =
    true

let start _ =
    true

[<EntryPoint>]
let main _ =
    Service.Default
    |> run_as_local_system
    |> start_auto
    |> enable_shutdown
    |> with_recovery (ServiceRecovery.Default |> restart (min configuration.Service.RestartIntervalInMinutes))
    |> with_start start
    |> with_stop stop
    |> description configuration.Service.Description
    |> display_name configuration.Service.ServiceName
    |> service_name configuration.Service.ServiceName
    |> run
