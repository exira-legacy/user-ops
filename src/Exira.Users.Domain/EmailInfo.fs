namespace Exira.Users.Domain

[<AutoOpen>]
module EmailInfo =
    open System
    open Chiron
    open Chiron.Operators

    type EmailInfo =
    | UnverifiedEmail of Email: Email.Email
    | VerifiedEmail of Email: Email.Email * VerifiedAt: DateTime
    with
        static member ToJson (e: EmailInfo) =
            match e with
            | UnverifiedEmail (Email = email) ->
                Json.write "email" email

            | VerifiedEmail (Email = email; VerifiedAt = dateVerified) ->
                Json.write "email" email
                *> Json.write "verifiedAt" dateVerified

        static member FromJson (_: EmailInfo) =
            (fun email verifiedAt ->
                match verifiedAt, email with
                | Some dateVerified, mail ->
                    VerifiedEmail (Email = mail, VerifiedAt = dateVerified)

                | None, mail ->
                    UnverifiedEmail (Email = mail))
            <!> Json.read "email"
            <*> Json.tryRead "verifiedAt"

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
