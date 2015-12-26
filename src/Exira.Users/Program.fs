open System.Reflection
open System.IO
open FSharp.Configuration
open Topshelf
open Time
open Akka.FSharp
open Akka.FSharp.Spawn
open Akka.Actor

let executablePath = Assembly.GetEntryAssembly().Location |> Path.GetDirectoryName
let configPath = Path.Combine(executablePath, "Configuration.yaml")

type Configuration = YamlConfig<"Configuration.yaml">
let configuration = Configuration()
configuration.Load configPath

let mutable (actorSystem: ActorSystem option) = None

type Message = { Text: string }

let dummyActor message =
    printfn "Got message: %s" message.Text

let stop _ =
    match actorSystem with
        | None -> true
        | Some system ->
            system.Shutdown()
            system.AwaitTermination()
            actorSystem <- None
            true

let start _ =
    let system = System.create configuration.Service.ServiceName (Configuration.load())

    let dummyActor = spawn system "dummy" (actorOf dummyActor)

    let hiMessage = { Text = "Hello!" }
    system.Scheduler.ScheduleTellRepeatedly(s 5, s 1, dummyActor, hiMessage)

    actorSystem <- Some system
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
