namespace Exira.Users.Projections.CreateDefaultAccount

[<AutoOpen>]
module internal Handle =
    open EventStore.ClientAPI
    open EventStore.ClientAPI.Exceptions
    open Exira.ErrorHandling
    open Exira.EventStore
    open Exira.EventStore.EventStore
    open Exira.Users.Domain
    open Exira.Users.Domain.Events

    let handleProjection es (_, state: User) =
        match state with
        | Init
        | Deleted -> succeed state

        | UnverifiedUser (User = user)
        | VerifiedUser (User = user) ->
            printfn "Create default account for %s" (user.Email |> EmailInfo.getEmail |> Email.value)

            let accountCreated =
                { AccountCreatedEvent.Type = AccountType.Personal
                  Name = user.PersonalAccount
                  Email = user.Email
                  Users = [ EmailInfo.getEmail user.Email ] } |> AccountCreated |> Event.Account

            let streamId = toAccountStreamId user.PersonalAccount

            let result =
                async {
                    try
                        do! appendToStream es streamId ExpectedVersion.NoStream [accountCreated]
                        return succeed id
                    with
                    | :? WrongExpectedVersionException as ex -> return fail [Error.SaveVersionException ex]
                    | ex -> return fail [Error.SaveException ex]
                } |> Async.RunSynchronously

            match result with
            | Success _ -> succeed state
            | Failure f -> f |> ProjectionProblem |> fail
