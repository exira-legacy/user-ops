namespace Exira.Users.Domain

[<AutoOpen>]
module AccountName =
    open Chiron

    let [<Literal>] PersonalAccountNamePrefix = "personal"

    type AccountName = AccountName of string

    let private canonicalizeName (name: string) =
        name.ToLower().Trim()

    // TODO: Add validation on accountname, remember to whitelist personal prefix

    let internal createWithCont success failure value =
        match value with
        | null -> failure StringError.Missing
        | _ -> success (value |> canonicalizeName |> AccountName)

    let internal create value =
        let success e = Some e
        let failure _  = None
        createWithCont success failure value

    let apply f (AccountName e) = f e

    let value e = apply id e

    let toJson (name: AccountName) =
        name
        |> value
        |> String

    let fromJson json =
        let error x =
            Json.formatWith JsonFormattingOptions.SingleLine x
            |> sprintf "couldn't deserialise to AccountName: %s"
            |> Error

        match json with
        | String name -> name |> AccountName |> Value
        | _ -> error json
