namespace Exira.Users.Domain

[<AutoOpen>]
module Password =
    // TODO: see if Serilog respects this
    type Password = Password of string override __.ToString() = ""

    let [<Literal>] private MinimumPasswordLength = 10

    let internal createWithCont success failure value =
        match value with
        | null -> failure StringError.Missing
        | MinimumLength MinimumPasswordLength _ -> success (value |> Password)
        | _ -> failure (TooShort MinimumPasswordLength)

    let internal create value =
        let success e = Some e
        let failure _  = None
        createWithCont success failure value

    let apply f (Password e) = f e

    let value e = apply id e