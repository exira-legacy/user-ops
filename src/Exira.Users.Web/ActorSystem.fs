namespace Exira.Users.Web

[<AutoOpen>]
module internal ActorSystem =
    open Akka.FSharp
    open Akka.Actor
    open Exira.Users.Domain

    let private configuration = Settings.configuration

    let bootstrapActorSystem () =
        let system = System.create configuration.Akka.Name (Configuration.load())

        async {
            let userActorPath = select "akka.tcp://user-ops@127.0.0.1:8901/user/user" system

            let! reply = userActorPath <? Identify("correlation-id")
            let userActor =
                match (reply: obj) with
                | :? ActorIdentity as identity when not(isNull identity.Subject) -> Some identity.Subject
                | _ -> None

            let loginCommand = { LoginCommand.Email = Email.create "test@test.be" |> Option.get; Password = Password.create "test123456789" |> Option.get }
            match userActor with
            | Some a ->  a <! loginCommand
            | None -> ()
        } |> Async.RunSynchronously

        Some system
