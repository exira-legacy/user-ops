namespace Exira.Users.Domain

[<AutoOpen>]
module Email =
    open System.Text.RegularExpressions
    open Chiron

    type Email = Email of string

    let [<Literal>] private SimpleEmailRegex = "^\S+@\S+\.\S+$"

    let private canonicalizeEmail (email: string) =
        Regex.Replace(email.ToLower(), "\s", " ").Trim()

    let internal createWithCont success failure value =
        match value with
        | null -> failure StringError.Missing
        | Match SimpleEmailRegex _ -> success (value |> canonicalizeEmail |> Email)
        | _ -> failure (DoesntMatchPattern SimpleEmailRegex)

    let internal create value =
        let success e = Some e
        let failure _  = None
        createWithCont success failure value

    let apply f (Email e) = f e

    let value e = apply id e

    let multipleToJson (emails: Email list) =
        emails
        |> List.map (fun email -> email |> value |> String)
        |> Array

    let multipleFromJson json =
        let error x =
            Json.formatWith JsonFormattingOptions.SingleLine x
            |> sprintf "couldn't deserialise to Email: %s"
            |> Error

        match json with
        | Array emails ->
            emails
            |> List.choose (function
                | String email -> email |> Email |> Some
                | _ -> None)
            |> Value
        | _ -> error json