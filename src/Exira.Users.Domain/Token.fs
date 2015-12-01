namespace Exira.Users.Domain

[<AutoOpen>]
module Token =
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
