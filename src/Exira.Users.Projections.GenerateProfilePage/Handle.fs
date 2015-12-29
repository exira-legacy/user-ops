namespace Exira.Users.Projections.GenerateProfilePage

[<AutoOpen>]
module internal Handle =
    open Exira.ErrorHandling
    open Exira.Users.Domain

    let handleProjection es (_, state: User) =
        match state with
        | Init
        | Deleted -> succeed state

        | UnverifiedUser (User = user)
        | VerifiedUser (User = user) ->
            printfn "Generate profile page for %s" (user.Email |> EmailInfo.getEmail |> Email.value)

            // TODO: Do something :)
            // async {
            //     let! state = Account.getAccountState user.PersonalAccount es
            // } |> Async.RunSynchronously

            succeed state
