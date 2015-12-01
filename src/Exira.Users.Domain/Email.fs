namespace Exira.Users.Domain

[<AutoOpen>]
module Email =
    open System.Text.RegularExpressions

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
