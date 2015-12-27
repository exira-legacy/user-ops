namespace Exira.Users

[<AutoOpen>]
module internal ActorSystem =
    open Akka.FSharp
    open Akka.FSharp.Spawn

    let private configuration = Settings.configuration

    let private dummyActor message =
        printfn "Got message: %s" message

    let bootstrapActorSystem () =
        let system = System.create configuration.Akka.Name (Configuration.load())

        let dummyActor = spawn system "dummy" (actorOf dummyActor)

        Some system
