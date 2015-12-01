namespace Exira.Users.Domain

[<AutoOpen>]
module Role =
    let [<Literal>] RoleUser = "User"
    let [<Literal>] RoleAdministrator = "Administrator"

    type RoleType =
    | User
    | Administrator
    with member this.toString =
            match this with
            | User -> RoleUser
            | Administrator -> RoleAdministrator

    type Role = Role of RoleType

    let internal createWithCont success failure value =
        match value with
        | null
        | String.empty -> failure StringError.Missing
        | RoleUser -> User |> Role |> success
        | RoleAdministrator -> Administrator |> Role |> success
        | _ -> failure (sprintf "%s|%s" RoleUser RoleAdministrator |> StringError.DoesntMatchPattern)

    let internal create value =
        let success e = Some e
        let failure _  = None
        createWithCont success failure value

    let apply f (Role e) = f e

    let value e = apply id e
