namespace Exira.Users.Domain

[<AutoOpen>]
module EmailInfo =
    open System
    open System.Globalization
    open Chiron

    type EmailInfo =
    | UnverifiedEmail of Email.Email
    | VerifiedEmail of Email.Email * DateTime

    // unverified on creation
    let create email =
        UnverifiedEmail email

    // handle the "verified" event
    let verified emailInfo dateVerified =
        match emailInfo with
        | UnverifiedEmail email -> VerifiedEmail (email, dateVerified)
        | VerifiedEmail _ -> emailInfo

    let toJson (emailInfo: EmailInfo) =
        match emailInfo with
        | UnverifiedEmail email ->
            Map.ofList [
                "verified", false |> Bool
                "email", email |> Email.value |> String ]
            |> Object

        | VerifiedEmail (email, dateVerified) ->
            Map.ofList [
                "verified", true |> Bool
                "email", email |> Email.value |> String
                "verifiedAt", dateVerified.ToString("o") |> String ]
            |> Object

    let fromJson x =
        match x with
        | Object values ->
            let verified = values |> Map.find "verified"
            let email = values |> Map.find "email"
            let verifiedAt = values |> Map.tryFind "verifiedAt"

            match verified, email, verifiedAt with
            | Bool v, String mail, Some (String dateVerified) when v ->
                VerifiedEmail ((Email.Email mail), DateTime.Parse(dateVerified, null, DateTimeStyles.RoundtripKind)) |> Value

            | Bool v, String mail, None when not v ->
                UnverifiedEmail (Email.Email mail) |> Value

            | _ ->
                Json.formatWith JsonFormattingOptions.SingleLine x
                |> sprintf "Expected a string containing a valid ISO-8601 date/time: %s"
                |> Error