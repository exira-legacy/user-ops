namespace Exira.Users.Web

[<AutoOpen>]
module internal ActorSystem =
    open System
    open Akka.FSharp

    let private configuration = Settings.configuration

    let bootstrapActorSystem () =
        let system = System.create configuration.Akka.Name (Configuration.load())

        let dummyActor =
            system
                .ActorSelection("akka.tcp://user-ops@127.0.0.1:8901/user/dummy")
                .ResolveOne(TimeSpan.FromSeconds 3.)
                .Result

        system.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds 5., TimeSpan.FromSeconds 1., dummyActor, "hello")

        Some system
