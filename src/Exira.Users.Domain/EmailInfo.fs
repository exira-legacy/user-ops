namespace Exira.Users.Domain

[<AutoOpen>]
module EmailInfo =
    open System
    open System.Globalization
    open Chiron

    type EmailInfo =
    | UnverifiedEmail of Email: Email.Email
    | VerifiedEmail of Email: Email.Email * VerifiedAt: DateTime

    // unverified on creation
    let create email =
        UnverifiedEmail (Email = email)

    // handle the "verified" event
    let verified emailInfo dateVerified =
        match emailInfo with
        | UnverifiedEmail (Email = email) -> VerifiedEmail (Email = email, VerifiedAt = dateVerified)
        | VerifiedEmail _ -> emailInfo

    let getEmail = function
        | UnverifiedEmail (Email = e)
        | VerifiedEmail (Email = e) -> e

    let toJson (emailInfo: EmailInfo) =
        match emailInfo with
        | UnverifiedEmail (Email = email) ->
            Map.ofList [
                "email", email |> Email.value |> String ]
            |> Object

        | VerifiedEmail (Email = email; VerifiedAt = dateVerified) ->
            Map.ofList [
                "email", email |> Email.value |> String
                "verifiedAt", dateVerified.ToString "o" |> String ]
            |> Object

    let fromJson json =
        let error x =
            Json.formatWith JsonFormattingOptions.SingleLine x
            |> sprintf "couldn't deserialise to EmailInfo: %s"
            |> Error

        match json with
        | Object values ->
            let email = values |> Map.find "email"
            let verifiedAt = values |> Map.tryFind "verifiedAt"

            match verifiedAt, email  with
            | Some (String dateVerified), String mail ->
                VerifiedEmail (Email = Email mail, VerifiedAt = DateTime.Parse(dateVerified, null, DateTimeStyles.RoundtripKind)) |> Value

            | None, String mail ->
                UnverifiedEmail (Email = Email mail) |> Value

            | _ -> error json
        | _ -> error json