namespace Exira.Users.Domain

module Commands =

    type Command =
    | User of UserCommand
    | Account of AccountCommand

    and UserCommand =
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

    and AccountCommand =
    | Create of CreateAccountCommand

    and CreateAccountCommand = {
        Type: AccountType
        Name: AccountName.AccountName
        Email: Email.Email
        Users: Email.Email list
    }