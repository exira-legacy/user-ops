namespace Exira.Users.Domain

[<AutoOpen>]
module Model =
    open System

    type VerificationToken = VerificationToken of Token.Token
    type PasswordResetToken = PasswordResetToken of Token.Token

    type PasswordResetInfo = {
        RequestedAt: DateTime
        ResetToken: PasswordResetToken
    }

    type UserInfo = {
        Email: EmailInfo.EmailInfo
        Hash: PasswordHash.PasswordHash
        Roles: Role.Role list
    }