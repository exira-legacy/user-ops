namespace Exira.Users.Domain

[<AutoOpen>]
module Events =
    open System

    type Event =
    | UserRegistered of UserRegisteredEvent
    | UserLoggedIn of UserLoggedInEvent
    | UserVerified of UserVerifiedEvent
    | PasswordChanged of PasswordChangedEvent
    | RequestedPasswordReset of RequestedPasswordResetEvent
    | VerifiedPasswordReset of VerifiedPasswordResetEvent

    and UserRegisteredEvent = {
        Email: EmailInfo.EmailInfo
        VerificationToken: VerificationToken
        Hash: PasswordHash.PasswordHash
        Roles: Role.Role list
    }

    and UserLoggedInEvent = {
        LoggedInAt: DateTime
    }

    and UserVerifiedEvent = {
        Email: EmailInfo.EmailInfo
    }

    and PasswordChangedEvent = {
        Hash: PasswordHash.PasswordHash
    }

    and RequestedPasswordResetEvent = {
        RequestedAt: DateTime
        ResetToken: PasswordResetToken
    }

    and VerifiedPasswordResetEvent = {
        VerifiedAt: DateTime
        Hash: PasswordHash.PasswordHash
    }