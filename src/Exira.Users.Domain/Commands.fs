namespace Exira.Users.Domain

module Commands =
    open Destructurama.Attributed

    // User
    type RegisterCommand = {
        Email: Email.Email
        [<NotLogged>] Password: Password.Password
    }

    type LoginCommand = {
        Email: Email.Email
        [<NotLogged>] Password: Password.Password
    }

    type VerifyCommand = {
        Email: Email.Email
        [<NotLogged>] Token: VerificationToken
    }

    type ChangePasswordCommand = {
        Email: Email.Email
        [<NotLogged>] PreviousPassword: Password.Password
        [<NotLogged>] NewPassword: Password.Password
    }

    type RequestPasswordResetCommand = {
        Email: Email.Email
    }

    type VerifyPasswordResetCommand = {
        Email: Email.Email
        [<NotLogged>] Token: PasswordResetToken
        [<NotLogged>] NewPassword: Password.Password
    }

    // Acount
    type CreateAccountCommand = {
        Type: AccountType
        Name: AccountName.AccountName
        Email: Email.Email
        Users: Email.Email list
    }

    type UserCommand =
    | Register of RegisterCommand
    | Login of LoginCommand
    | Verify of VerifyCommand
    | ChangePassword of ChangePasswordCommand
    | RequestPasswordReset of RequestPasswordResetCommand
    | VerifyPasswordReset of VerifyPasswordResetCommand

    type AccountCommand =
    | Create of CreateAccountCommand

    type Command =
    | User of Command: UserCommand
    | Account of Command: AccountCommand
