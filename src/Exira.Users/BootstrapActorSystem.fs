namespace Exira.Users

[<AutoOpen>]
module internal BootstrapActorSystem =
    open Akka.FSharp
    open Akka.FSharp.Spawn
    open Topshelf.FSharpApi.Time

    let private configuration = Settings.configuration

    type private Message = { Text: string }

    let private dummyActor message =
        printfn "Got message: %s" message.Text

    let bootstrap () =
        let system = System.create configuration.Service.ServiceName (Configuration.load())

        let dummyActor = spawn system "dummy" (actorOf dummyActor)

        let hiMessage = { Text = "Hello!" }
        system.Scheduler.ScheduleTellRepeatedly(s 5, s 1, dummyActor, hiMessage)

        Some system
