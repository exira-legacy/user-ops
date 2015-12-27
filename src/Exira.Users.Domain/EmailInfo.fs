namespace Exira.Users.Domain

[<AutoOpen>]
module EmailInfo =
    open System

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
