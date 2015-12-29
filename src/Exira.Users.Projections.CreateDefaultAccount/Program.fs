namespace Exira.Users.Projections.CreateDefaultAccount

module Program =
    open System
    open EventStore.ClientAPI
    open Exira.ErrorHandling
    open Exira.EventStore
    open Exira.EventStore.EventStore
    open Exira.Users.Domain

    let private projectionConfig = Configuration.projectionConfig

    let private es = connect projectionConfig.EventStore.ConnectionString |> Async.RunSynchronously

    let private checkpointStream = sprintf "%s-checkpoint" projectionConfig.Projection.Name |> StreamId

    let private map error =
        match error with
        | InvalidEvent -> "Saw an event we dont care about"
        | ValidateProblem e -> e.ToString() |> sprintf "Validate problem: '%s'"
        | DeserializeProblem e -> e.ToString() |> sprintf "Serializer problem: '%s'"
        | StateProblem errors -> formatErrors errors  |> sprintf "State problem: '%A'"
        | ProjectionProblem errors -> formatErrors errors  |> sprintf "Projection problem: '%A'"

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

    [<EntryPoint>]
    let main _ =
        initalizeCheckpoint es checkpointStream |> Async.RunSynchronously
        storeCheckpoint es checkpointStream Position.Start |> Async.RunSynchronously

        subscribe subscriptionDropped |> ignore

        Console.ReadLine() |> ignore

        es.Close()

        0
