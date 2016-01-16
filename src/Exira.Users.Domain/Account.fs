namespace Exira.Users.Domain

open Exira.ErrorHandling

module Account =
    open Events

    let private applyAccountEvent state event =
        match state with
        | Account.Init ->
            match event with
            | AccountCreated e ->
                let account =
                    { AccountInfo.Name = e.Name
                      Email = e.Email
                      Users = e.Users }

                match e.Type with
                | Personal -> PersonalAccount (Account = account) |> Success
                | Company -> CompanyAccount (Account = account) |> Success

        | Account.PersonalAccount account ->
            match event with
            | AccountCreated _ -> stateTransitionFail event state

        | Account.CompanyAccount account ->
            match event with
            | AccountCreated _ -> stateTransitionFail event state

        | Account.Deleted ->
            match event with
            | AccountCreated _ -> stateTransitionFail event state

    let private applyEvent state event =
        match event with
        | Event.Account e -> applyAccountEvent state e
        | _ -> stateTransitionFail event state

    let getAccountState id = getState (applyEvents applyEvent) Account.Init (toAccountStreamId id)

module internal AccountCommandHandler =
    open EventStore.ClientAPI
    open Commands
    open Events
    open Account

    let private logger = Logging.logger

    let private createAccount (command: CreateAccountCommand) (_, state) =
        logger.Information("Creating account {name}", command.Name)
        logger.Debug("AccountCommandHandler.createAccount: {@command}, current state: {@state}", command, state)

        // An account can only be created when it does not exist yet
        match state with
        | Account.Init ->
            let accountCreated =
                { AccountCreatedEvent.Name = command.Name
                  Email = EmailInfo.create command.Email
                  Users = command.Users
                  Type = command.Type } |> AccountCreated |> Event.Account

            let streamId = toAccountStreamId command.Name
            let response = Response.AccountCreated streamId

            logger.Information("Created account {name}", command.Name)
            logger.Debug("Generated AccountCreatedEvent {@event}", accountCreated)
            succeed (streamId, ExpectedVersion.NoStream, [accountCreated], response)

        | Account.PersonalAccount _
        | Account.CompanyAccount _
        | Account.Deleted _ ->
            logger.Information("Cannot create account {name}, it already exists", command.Name)
            fail [AccountAlreadyExists]

    let handleCreateAccount (command: CreateAccountCommand) es =
        async {
            let! state = getAccountState command.Name es

            return!
                state
                |> bind (createAccount command)
                |> bindAsync (save es)
        }