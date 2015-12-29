namespace Exira.Users.Domain

[<AutoOpen>]
module Role =
    open Chiron

    let [<Literal>] private RoleUser = "User"
    let [<Literal>] private RoleAdministrator = "Administrator"
    let private roles =
        [ RoleUser
          RoleAdministrator ]

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
        | _ -> failure (roles |> String.concat "|" |> StringError.DoesntMatchPattern)

    let internal create value =
        let success e = Some e
        let failure _  = None
        createWithCont success failure value

    let apply f (Role e) = f e

    let value e = apply id e

    let toJson (roles: Role list) =
        roles
        |> List.map (fun role -> (role |> value).toString |> String)
        |> Array

    let fromJson json =
        let error x =
            Json.formatWith JsonFormattingOptions.SingleLine x
            |> sprintf "couldn't deserialise to Roles: %s"
            |> Error

        match json with
        | Array roles ->
            roles
            |> List.choose (function
                | String role -> Some role
                | _ -> None)
            |> List.choose (function
                | RoleUser -> User |> Role |> Some
                | RoleAdministrator -> Administrator |> Role |> Some
                | _ -> None)
            |> Value

        | _ -> error json