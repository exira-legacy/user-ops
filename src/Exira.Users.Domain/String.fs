namespace Exira.Users.Domain

[<AutoOpen>]
module String =
    open System

    let [<Literal>] private Characters = "abcdefghijklmnopqrstuvwxyz0123456789"

    let private charsLen = Characters.Length
    let private rng = Random()

    /// Create a random string consisting of the lower case alphabet and 0-9
    let random length =
        String [| for _ in 0..length -> Characters.[rng.Next(charsLen)] |]
