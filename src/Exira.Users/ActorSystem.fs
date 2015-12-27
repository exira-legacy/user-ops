namespace Exira.Users

[<AutoOpen>]
module internal ActorSystem =
    open Akka.FSharp
    open Akka.FSharp.Spawn
    open Akka.Routing
    open Exira.Users.Domain

    let private configuration = Settings.configuration

    let private dummyActor message =
        printfn "Got message: %s" message

    let private userActor message =
        match message with
        | Register userCommand -> printfn "Register: %A" message
        | Login userCommand -> printfn "Login: %A" message
        | _ -> printfn "Message: %A" message
        ()

    let bootstrapActorSystem () =
        let system = System.create configuration.Akka.Name (Configuration.load())

        let dummyActor = spawn system "dummy" (actorOf dummyActor)

        let userHash = ConsistentHashingPool 5
        let userHash = userHash.WithHashMapping(fun msg ->
            match msg with
            | :? Commands.RegisterCommand as cmd -> cmd.Email |> Email.value :> obj
            | :? Commands.LoginCommand as cmd -> cmd.Email |> Email.value :> obj
            | _ -> null
        )
        let userActor = spawnOpt system "user" (actorOf userActor) [SpawnOption.Router(userHash)]

        Some system
