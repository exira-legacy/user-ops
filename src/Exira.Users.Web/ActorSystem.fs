namespace Exira.Users.Web

[<AutoOpen>]
module internal ActorSystem =
    open System
    open Akka.FSharp
    open Akka.Actor

    let private configuration = Settings.configuration

    let bootstrapActorSystem () =
        let system = System.create configuration.Akka.Name (Configuration.load())

        async {
            let dummyActorPath = select "akka.tcp://user-ops@127.0.0.1:8901/user/dummy" system

            let! reply = dummyActorPath <? Identify("correlation-id")
            let dummyActor =
                match (reply: obj) with
                | :? ActorIdentity as identity when not(isNull identity.Subject) -> Some identity.Subject
                | _ -> None

            match dummyActor with
            | Some a ->  system.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds 5., TimeSpan.FromSeconds 1., a, "hello")
            | None -> ()
        } |> Async.RunSynchronously

        Some system
