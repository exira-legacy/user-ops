namespace Exira.Users

module Model =
    open Exira.ErrorHandling
    open Exira.Users.Domain

    type Dto =
    | Register of RegisterDto
    | Login of LoginDto
    | Verify of VerifyDto
    | ChangePassword of ChangePasswordDto
    | RequestPasswordReset of RequestPasswordResetDto
    | VerifyPasswordReset of VerifyPasswordResetDto

    and [<CLIMutable>] RegisterDto = {
        Email: string
        Password: string
    }

    and [<CLIMutable>] LoginDto = {
        Email: string
        Password: string
    }

    and [<CLIMutable>] VerifyDto = {
        Email: string
        Token: string
    }

    and [<CLIMutable>] ChangePasswordDto = {
        Email: string
        PreviousPassword: string
        NewPassword: string
    }

    and [<CLIMutable>] RequestPasswordResetDto = {
        Email: string
    }

    and [<CLIMutable>] VerifyPasswordResetDto = {
        Email: string
        Token: string
        NewPassword: string
    }

    let toCommand dto =
        match dto with
        | Dto.Register d ->
            let emailOrFail = createEmail d.Email
            let passwordOrFail = createPassword d.Password

            let createRegister email password =
                Command.Register {
                    RegisterCommand.Email = email
                    Password = password
                }

            createRegister
            <!> emailOrFail
            <*> passwordOrFail

        | Dto.Login d ->
            let emailOrFail = createEmail d.Email
            let passwordOrFail p = createPassword p

            let createLogin email password =
                Command.Login {
                    LoginCommand.Email = email
                    Password = password
                }

            createLogin
            <!> emailOrFail
            <*> passwordOrFail d.Password

        | Dto.Verify d ->
            let emailOrFail = createEmail d.Email
            let tokenOrFail = createToken d.Token

            let createVerify email token =
                Command.Verify {
                    VerifyCommand.Email = email
                    Token = VerificationToken token
                }

            createVerify
            <!> emailOrFail
            <*> tokenOrFail

        | Dto.ChangePassword d ->
            let emailOrFail = createEmail d.Email
            let passwordOrFail p = createPassword p

            let createChangePassword email oldPassword newPassword =
                Command.ChangePassword {
                    ChangePasswordCommand.Email = email
                    PreviousPassword = oldPassword
                    NewPassword = newPassword
                }

            createChangePassword
            <!> emailOrFail
            <*> passwordOrFail d.PreviousPassword
            <*> passwordOrFail d.NewPassword

        | Dto.RequestPasswordReset d ->
            let emailOrFail = createEmail d.Email

            let createRequestPasswordReset email =
                Command.RequestPasswordReset {
                    RequestPasswordResetCommand.Email = email
                }

            createRequestPasswordReset
            <!> emailOrFail

        | Dto.VerifyPasswordReset d ->
            let emailOrFail = createEmail d.Email
            let tokenOrFail = createToken d.Token
            let passwordOrFail = createPassword d.NewPassword

            let createVerifyPasswordReset email token password =
                Command.VerifyPasswordReset {
                    VerifyPasswordResetCommand.Email = email
                    Token = PasswordResetToken token
                    NewPassword = password
                }

            createVerifyPasswordReset
            <!> emailOrFail
            <*> tokenOrFail
            <*> passwordOrFail
