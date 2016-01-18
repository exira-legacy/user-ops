namespace Exira.Users

module Model =
    open Exira.ErrorHandling
    open Exira.Users.Domain
    open Exira.Users.Domain.Commands
    open Destructurama.Attributed

    type [<CLIMutable>] RegisterDto = {
        Email: string
        [<NotLogged>] Password: string
    }

    type [<CLIMutable>] LoginDto = {
        Email: string
        [<NotLogged>] Password: string
    }

    type [<CLIMutable>] VerifyDto = {
        Email: string
        [<NotLogged>] Token: string
    }

    type [<CLIMutable>] ChangePasswordDto = {
        Email: string
        [<NotLogged>] PreviousPassword: string
        [<NotLogged>] NewPassword: string
    }

    type [<CLIMutable>] RequestPasswordResetDto = {
        Email: string
    }

    type [<CLIMutable>] VerifyPasswordResetDto = {
        Email: string
        [<NotLogged>] Token: string
        [<NotLogged>] NewPassword: string
    }

    type Dto =
    | Register of RegisterDto
    | Login of LoginDto
    | Verify of VerifyDto
    | ChangePassword of ChangePasswordDto
    | RequestPasswordReset of RequestPasswordResetDto
    | VerifyPasswordReset of VerifyPasswordResetDto

    let private toUserCommand cmd =
        cmd |> Command.User

    let toCommand dto =
        match dto with
        | Dto.Register d ->
            let emailOrFail = createEmail d.Email
            let passwordOrFail = createPassword d.Password

            let createRegister email password =
                UserCommand.Register {
                    RegisterCommand.Email = email
                    Password = password
                } |> toUserCommand

            createRegister
            <!> emailOrFail
            <*> passwordOrFail

        | Dto.Login d ->
            let emailOrFail = createEmail d.Email
            let passwordOrFail p = createPassword p

            let createLogin email password =
                UserCommand.Login {
                    LoginCommand.Email = email
                    Password = password
                } |> toUserCommand

            createLogin
            <!> emailOrFail
            <*> passwordOrFail d.Password

        | Dto.Verify d ->
            let emailOrFail = createEmail d.Email
            let tokenOrFail = createToken d.Token

            let createVerify email token =
                UserCommand.Verify {
                    VerifyCommand.Email = email
                    Token = VerificationToken token
                } |> toUserCommand

            createVerify
            <!> emailOrFail
            <*> tokenOrFail

        | Dto.ChangePassword d ->
            let emailOrFail = createEmail d.Email
            let passwordOrFail p = createPassword p

            let createChangePassword email oldPassword newPassword =
                UserCommand.ChangePassword {
                    ChangePasswordCommand.Email = email
                    PreviousPassword = oldPassword
                    NewPassword = newPassword
                } |> toUserCommand

            createChangePassword
            <!> emailOrFail
            <*> passwordOrFail d.PreviousPassword
            <*> passwordOrFail d.NewPassword

        | Dto.RequestPasswordReset d ->
            let emailOrFail = createEmail d.Email

            let createRequestPasswordReset email =
                UserCommand.RequestPasswordReset {
                    RequestPasswordResetCommand.Email = email
                } |> toUserCommand

            createRequestPasswordReset
            <!> emailOrFail

        | Dto.VerifyPasswordReset d ->
            let emailOrFail = createEmail d.Email
            let tokenOrFail = createToken d.Token
            let passwordOrFail = createPassword d.NewPassword

            let createVerifyPasswordReset email token password =
                UserCommand.VerifyPasswordReset {
                    VerifyPasswordResetCommand.Email = email
                    Token = PasswordResetToken token
                    NewPassword = password
                } |> toUserCommand

            createVerifyPasswordReset
            <!> emailOrFail
            <*> tokenOrFail
            <*> passwordOrFail
