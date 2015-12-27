namespace Exira.Users

module Program =
    open Topshelf
    open Time
    open Akka.Actor

    let private configuration = Settings.configuration

    let mutable (actorSystem: ActorSystem option) = None

    let stop _ =
        match actorSystem with
            | None -> true
            | Some system ->
                system.Shutdown()
                system.AwaitTermination()
                actorSystem <- None
                true

    let start _ =
        actorSystem <- bootstrapActorSystem()
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
