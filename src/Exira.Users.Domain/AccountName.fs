namespace Exira.Users.Domain

[<AutoOpen>]
module AccountName =
    type AccountName = AccountName of string

    // TODO: Add validation on accountname

    let internal createWithCont success failure value =
        match value with
        | null -> failure StringError.Missing
        | _ -> success (value |> AccountName)

    let internal create value =
        let success e = Some e
        let failure _  = None
        createWithCont success failure value

    let apply f (AccountName e) = f e

    let value e = apply id e
