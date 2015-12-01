namespace Exira.Users.Domain

[<AutoOpen>]
module Commands =

    type Command =
    | Register of RegisterCommand
    | Login of LoginCommand
    | Verify of VerifyCommand
    | ChangePassword of ChangePasswordCommand
    | RequestPasswordReset of RequestPasswordResetCommand
    | VerifyPasswordReset of VerifyPasswordResetCommand

    and RegisterCommand = {
        Email: Email.Email
        Password: Password.Password
    }

    and LoginCommand = {
        Email: Email.Email
        Password: Password.Password
    }

    and VerifyCommand = {
        Email: Email.Email
        Token: VerificationToken
    }

    and ChangePasswordCommand = {
        Email: Email.Email
        PreviousPassword: Password.Password
        NewPassword: Password.Password
    }

    and RequestPasswordResetCommand = {
        Email: Email.Email
    }

    and VerifyPasswordResetCommand = {
        Email: Email.Email
        Token: PasswordResetToken
        NewPassword: Password.Password
    }