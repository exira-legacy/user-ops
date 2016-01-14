namespace Exira.Users.Projections.CreateDefaultAccount

module Program =
    open EventStore.ClientAPI
    open Exira.ErrorHandling
    open Exira.EventStore
    open Exira.EventStore.EventStore
    open Exira.Users.Domain
    open Topshelf
    open Time

    let private projectionConfig = Configuration.projectionConfig
    let private logger = Serilogger.logger
    let private checkpointStreamName = sprintf "%s-checkpoint" projectionConfig.Projection.Name
    let private checkpointStream = checkpointStreamName |> StreamId

    let mutable (es: IEventStoreConnection option) = None

    let private map error =
        match error with
        | InvalidEvent -> "Saw an event we dont care about"
        | ValidateProblem e -> e.ToString() |> sprintf "Validate problem: '%s'"
        | DeserializeProblem e -> e.ToString() |> sprintf "Serializer problem: '%s'"
        | StateProblem errors -> formatErrors errors  |> sprintf "State problem: '%A'"
        | ProjectionProblem errors -> formatErrors errors  |> sprintf "Projection problem: '%A'"

    let private handleResult (resolvedEvent: ResolvedEvent) esConnection handledEvent =
        match handledEvent with
        | Success _ ->
            printfn "%04i@%s" resolvedEvent.Event.EventNumber resolvedEvent.Event.EventStreamId

            let result = storeCheckpoint esConnection checkpointStream resolvedEvent.OriginalPosition.Value |> Async.Catch |> Async.RunSynchronously
            match result with
            | Choice1Of2 _ -> ()
            | Choice2Of2 ex ->
                match ex with
                | :? System.AggregateException as aex ->
                    match aex.InnerException with
                    | :? System.ObjectDisposedException -> () // The connection has already been closed, but there are still events incoming
                    | _ -> raise aex.InnerException
                | _ -> raise ex

        | Failure error ->
            match  error with
            | InvalidEvent -> ()
            | _ ->
                // TODO: On failure, should either retry, or stop processing
                printfn "%s - %04i@%s" (map error) resolvedEvent.Event.EventNumber resolvedEvent.Event.EventStreamId

    let private eventAppeared esConnection = fun _ resolvedEvent ->
        logger.Debug("Event appeared for {projection}: {@event}", projectionConfig.Projection.Name, resolvedEvent)
        resolvedEvent
        |> handleEvent esConnection
        |> Async.RunSynchronously
        |> handleResult resolvedEvent esConnection

    let private subscribe esConnection = fun reconnect ->
        logger.Debug("Subscribing {projection}", projectionConfig.Projection.Name)
        let lastPosition = getCheckpoint esConnection checkpointStream |> Async.RunSynchronously
        logger.Debug("Subscribing {projection} at {@position}", projectionConfig.Projection.Name, lastPosition)
        subscribeToAllFrom esConnection lastPosition true (eventAppeared esConnection) ignore reconnect |> ignore
        logger.Debug("Subscribed {projection} at {@position}", projectionConfig.Projection.Name, lastPosition)

    let rec private subscriptionDropped esConnection = fun _ reason ex  ->
        printfn "Subscription Dropped: %O - %O" reason ex
        logger.Error("Subscription Dropped {reason} - {exception}", reason, ex.ToString())
        if reason = SubscriptionDropReason.ConnectionClosed then ()
        else subscribe esConnection (subscriptionDropped esConnection) |> ignore

    let private stop _ =
        logger.Information("Stopping {projection}", projectionConfig.Projection.Name)

        match es with
            | None -> ()
            | Some esConnection ->
                esConnection.Close()
                es <- None

        logger.Information("Stopped {projection}", projectionConfig.Projection.Name)
        true

    let private start _ =
        logger.Information("Starting {projection}", projectionConfig.Projection.Name)

        let esConnection = connect projectionConfig.EventStore.ConnectionString |> Async.RunSynchronously
        initalizeCheckpoint esConnection checkpointStream |> Async.RunSynchronously
        subscribe esConnection (subscriptionDropped esConnection) |> ignore
        es <- Some esConnection

        logger.Information("Started {projection}", projectionConfig.Projection.Name)
        true

    [<EntryPoint>]
    let main _ =
        let service =
            Service.Default
            |> run_as_local_system
            |> start_auto
            |> enable_shutdown
            |> with_recovery (ServiceRecovery.Default |> restart (min projectionConfig.Service.RestartIntervalInMinutes))
            |> with_start start
            |> with_stop stop
            |> description projectionConfig.Service.Description
            |> display_name projectionConfig.Service.ServiceName
            |> service_name projectionConfig.Service.ServiceName

        let service =
            if projectionConfig.Service.HasDependencies then
                projectionConfig.Service.DependsOn
                |> Seq.fold (fun s dependency -> s |> depends_on dependency) service
            else service

        service
        |> run
