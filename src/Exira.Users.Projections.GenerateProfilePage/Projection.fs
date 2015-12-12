namespace Exira.Users.Projections.GenerateProfilePage

[<AutoOpen>]
module internal Errors =
    type EventStoreError =
    | InvalidEvent
    | ValidateProblem of exn
    | DeserializeProblem of exn

[<AutoOpen>]
module internal Projection =
    open Microsoft.FSharp.Reflection
    open EventStore.ClientAPI
    open Exira.ErrorHandling
    open Exira.EventStore.Serialization
    open Exira.Users.Domain

    let private getUnionCaseName (e: 'a) = (FSharpValue.GetUnionFields(e, typeof<'a>) |> fst).Name

    let private userRegistered = getUnionCaseName (Event.UserRegistered Unchecked.defaultof<Events.UserRegisteredEvent>)

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

    let private handle es event =
        async {
            // TODO: Do something :)
            return succeed event
        }

    let handleEvent es event =
        event
        |> validateEvent
        |> bind deserializeEvent
        |> bindAsync (handle es)
