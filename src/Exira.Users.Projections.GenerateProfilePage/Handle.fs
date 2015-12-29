namespace Exira.Users.Projections.GenerateProfilePage

[<AutoOpen>]
module internal Handle =
    open Exira.ErrorHandling
    open Exira.Users.Domain

    let private projectionConfig = Configuration.projectionConfig

    let handleProjection es (_, state: User) =
        match state with
        | Init
        | Deleted -> succeed state

        | UnverifiedUser (User = user)
        | VerifiedUser (User = user) ->
            printfn "Generate profile page for %s" (user.Email |> EmailInfo.getEmail |> Email.value)

            let account =
                async {
                    return! Account.getAccountState user.PersonalAccount es
                } |> Async.RunSynchronously

            // TODO: Generate home page in site root
            //projectionConfig.Generator.SiteRoot

            succeed state
