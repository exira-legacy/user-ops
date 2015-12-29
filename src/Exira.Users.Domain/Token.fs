namespace Exira.Users.Domain

[<AutoOpen>]
module Token =
    open Chiron
    open Chiron.Operators

    type Token = Token of string
    with
        static member ToJson ((Token x): Token) = Json.Optic.set Json.String_ x
        static member FromJson (_: Token) = Token <!> Json.Optic.get Json.String_

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
