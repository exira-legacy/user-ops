namespace Exira.Users.Projections.GenerateProfilePage

[<AutoOpen>]
module internal Errors =
    open Exira.Users.Domain

    type EventStoreError =
    | InvalidEvent
    | ValidateProblem of exn
    | DeserializeProblem of exn
    | StateProblem of Error list

[<AutoOpen>]
module internal Projection =
    open EventStore.ClientAPI
    open Exira.ErrorHandling
    open Exira.EventStore.Serialization
    open Exira.Users.Domain
    open Exira.Users.Domain.Events

    let private userRegistered =
        UserEvent.UserRegistered Unchecked.defaultof<UserRegisteredEvent>
        |> Event.User
        |> generateEventType

    let private validateEvent (resolvedEvent: ResolvedEvent) =
        try
            if resolvedEvent.OriginalStreamId.StartsWith "$" then fail InvalidEvent
            else
                match resolvedEvent.OriginalEvent.EventType with
                | x when x = userRegistered -> succeed resolvedEvent
                | _ -> fail InvalidEvent
        with
        | ex -> ex |> ValidateProblem |> fail

    let private deserializeEvent (resolvedEvent: ResolvedEvent) =
        try
            resolvedEvent
            |> deserialize<Event>
            |> succeed
        with
        | ex -> ex |> DeserializeProblem |> fail

    let private getEmail emailInfo =
        match emailInfo with
        | UnverifiedEmail (Email = e)
        | VerifiedEmail (Email = e) -> e

    let private handleProjection (_, state: User) =
        match state with
        | Init
        | Deleted -> succeed state

        | UnverifiedUser (User = user)
        | VerifiedUser (User = user) ->
            // TODO: Do something :)
            printfn "Generate profile page for %s" (user.Email |> getEmail |> Email.value)
            succeed state

    let private handle es (event: Event) =
        async {
            let id =
                match event with
                | Event.User e ->
                    match e with
                    | UserEvent.UserRegistered e -> getEmail e.Email
                    | _ -> failwith "Shouldnt happen, it means our validate let something through..."
                | _ -> failwith "Shouldnt happen, it means our validate let something through..."

            let! state = User.getUserState id es

            match state with
            | Failure e -> return e |> StateProblem  |> fail
            | Success s -> return s |> handleProjection
        }

    let handleEvent es event =
        event
        |> validateEvent
        |> bind deserializeEvent
        |> bindAsync (handle es)
