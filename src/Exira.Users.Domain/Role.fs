namespace Exira.Users.Domain

[<AutoOpen>]
module Role =
    open Chiron
    open Chiron.Operators

    let [<Literal>] private RoleUser = "User"
    let [<Literal>] private RoleAdministrator = "Administrator"
    let private roles =
        [ RoleUser
          RoleAdministrator ]

    type RoleType =
    | User
    | Administrator
        member this.toString =
            match this with
            | User -> RoleUser
            | Administrator -> RoleAdministrator

    type Role = Role of RoleType
    with
        static member ToJson ((Role x): Role) = Json.Optic.set Json.String_ x.toString

        static member FromJson (_: Role) =
            function
                | RoleUser -> User |> Role
                | RoleAdministrator -> Administrator |> Role
                | x -> failwithf "couldn't deserialise %s to Role" x
            <!> Json.Optic.get Json.String_

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
