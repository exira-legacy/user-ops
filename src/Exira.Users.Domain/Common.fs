namespace Exira.Users.Domain

[<AutoOpen>]
module internal Errors =
    type GuidError =
    | Missing
    | MustNotBeEmpty

    type StringError =
    | Missing
    | TooShort of int
    | TooLong of int
    | DoesntMatchPattern of string

    type UriError =
    | Missing
    | Unknown

[<AutoOpen>]
module internal ActivePatterns =
    open System.Text.RegularExpressions

    let (|Match|_|) pattern input =
        let m = Regex.Match(input, pattern) in
        if m.Success then Some (List.tail [ for g in m.Groups -> g.Value ]) else None

    let (|MinimumLength|_|) length (input: string) =
        if input.Length >= length then Some input
        else None

    let (|MaximumLength|_|) length (input: string) =
        if input.Length <= length then Some input
        else None
