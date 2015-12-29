namespace Exira.Users.Domain

[<AutoOpen>]
module AccountType =
    open Chiron
    open Chiron.Operators

    let [<Literal>] private PersonalKey = "personal"
    let [<Literal>] private CompanyKey = "company"

    type AccountType =
    | Personal
    | Company
    with
        static member ToJson (e: AccountType) = Json.Optic.set Json.String_ e.toString

        static member FromJson (_: AccountType) =
            function
                | PersonalKey -> Personal
                | CompanyKey -> Company
                | x -> failwithf "couldn't deserialise %s to AccountType" x
            <!> Json.Optic.get Json.String_

        member this.toString =
            match this with
            | Personal -> PersonalKey
            | Company -> CompanyKey