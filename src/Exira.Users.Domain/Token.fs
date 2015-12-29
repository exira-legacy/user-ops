namespace Exira.Users.Domain

[<AutoOpen>]
module Token =
    open Chiron

    type Token = Token of string

    let internal createWithCont success failure value =
        match value with
        | null
        | "" -> failure StringError.Missing
        | _ -> success (value.ToLowerInvariant() |> Token)

    // TODO: Add some validation on token (length?)

    let internal create value =
        let success e = Some e
        let failure _  = None
        createWithCont success failure value

    let apply f (Token e) = f e

    let value e = apply id e

    let toJson (token: Token) =
        token
        |> value
        |> String

    let fromJson json =
        let error x =
            Json.formatWith JsonFormattingOptions.SingleLine x
            |> sprintf "couldn't deserialise to Token: %s"
            |> Error

        match json with
        | String token -> token |> Token |> Value
        | _ -> error json