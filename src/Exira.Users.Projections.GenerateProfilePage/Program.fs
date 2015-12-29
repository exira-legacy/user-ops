namespace Exira.Users.Projections.GenerateProfilePage

module Program =
    open System
    open FSharp.Configuration
    open EventStore.ClientAPI
    open Exira.ErrorHandling
    open Exira.EventStore
    open Exira.EventStore.EventStore
    open Exira.Users.Domain

    type private ProjectionConfig = YamlConfig<"Projection.yaml">

    let private projectionConfig = ProjectionConfig()

    let private es = connect projectionConfig.EventStore.ConnectionString |> Async.RunSynchronously

    let private checkpointStream = sprintf "%s-checkpoint" projectionConfig.Projection.Name |> StreamId

    let private map error =
        match error with
        | InvalidEvent -> "Saw an event we dont care about"
        | ValidateProblem e -> e.ToString() |> sprintf "Validate problem: '%s'"
        | DeserializeProblem e -> e.ToString() |> sprintf "Serializer problem: '%s'"
        | StateProblem errors -> formatErrors errors  |> sprintf "State problem: '%A'"

    let private handleResult (resolvedEvent: ResolvedEvent) handledEvent =
        match handledEvent with
        | Success _ ->
            printfn "%04i@%s" resolvedEvent.Event.EventNumber resolvedEvent.Event.EventStreamId
            storeCheckpoint es checkpointStream resolvedEvent.OriginalPosition.Value |> Async.RunSynchronously

        | Failure error ->
            match  error with
            | InvalidEvent -> ()
            | _ ->
                // TODO: On failure, should either retry, or stop processing
                printfn "%s - %04i@%s" (map error) resolvedEvent.Event.EventNumber resolvedEvent.Event.EventStreamId

    let private eventAppeared = fun _ resolvedEvent ->
        resolvedEvent
        |> handleEvent es
        |> Async.RunSynchronously
        |> handleResult resolvedEvent

    let private subscribe = fun reconnect ->
        let lastPosition = getCheckpoint es checkpointStream |> Async.RunSynchronously

        subscribeToAllFrom es lastPosition true eventAppeared ignore reconnect

    let rec private subscriptionDropped = fun _ reason ex  ->
        printfn "Subscription Dropped: %O - %O" reason ex
        subscribe subscriptionDropped |> ignore

    open Chiron

    [<EntryPoint>]
    let main _ =
        let printEvent e =
            e
            |> Json.serialize
            |> Json.formatWith JsonFormattingOptions.Pretty
            |> printfn "%s"

        let deserializedEvent json : Exira.Users.Domain.Events.Event =
            json
            |> Json.parse
            |> Json.deserialize

        { Exira.Users.Domain.Events.UserRegisteredEvent.Email = EmailInfo.VerifiedEmail (Email = Email.Email ("test@test.be"), VerifiedAt = DateTime.Now)
          Exira.Users.Domain.Events.UserRegisteredEvent.VerificationToken = VerificationToken (Token.Token ("bla"))
          Exira.Users.Domain.Events.UserRegisteredEvent.Hash = PasswordHash ("test")
          Exira.Users.Domain.Events.UserRegisteredEvent.Roles = [] }
        |> Exira.Users.Domain.Events.UserEvent.UserRegistered
        |> Exira.Users.Domain.Events.Event.User
        |> printEvent

        """
        {
          "user": {
            "userRegistered": {
              "emailInfo": {
                "email": "test@test.be",
                "verified": true,
                "verifiedAt": "2015-12-28T23:24:33.7496111+01:00"
              },
              "hash": "test",
              "token": "bla",
              "roles": []
            }
          }
        }
        """
        |> deserializedEvent
        |> printEvent

        { Exira.Users.Domain.Events.UserRegisteredEvent.Email = EmailInfo.UnverifiedEmail (Email = Email.Email ("test@test.be"))
          Exira.Users.Domain.Events.UserRegisteredEvent.VerificationToken = VerificationToken (Token.Token ("bla"))
          Exira.Users.Domain.Events.UserRegisteredEvent.Hash = PasswordHash ("test")
          Exira.Users.Domain.Events.UserRegisteredEvent.Roles = [ Role Administrator ] }
        |> Exira.Users.Domain.Events.UserEvent.UserRegistered
        |> Exira.Users.Domain.Events.Event.User
        |> printEvent

        """
        {
          "user": {
            "userRegistered": {
              "emailInfo": {
                "email": "test@test.be",
                "verified": false
              },
              "hash": "test",
              "token": "bla",
              "roles": ["Administrator", "User"]
            }
          }
        }
        """
        |> deserializedEvent
        |> printEvent

//        initalizeCheckpoint es checkpointStream |> Async.RunSynchronously
//        storeCheckpoint es checkpointStream Position.Start |> Async.RunSynchronously
//
//        subscribe subscriptionDropped |> ignore
//
//        Console.ReadLine() |> ignore
//
//        es.Close()

        0
