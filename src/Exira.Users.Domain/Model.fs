namespace Exira.Users.Domain

[<AutoOpen>]
module Model =
    open System.Security.Claims
    open Newtonsoft.Json
    open Exira.EventStore
    open System

    type ValidationError = {
        [<JsonProperty(PropertyName = "error")>]
        Message: string;
        Inner: ValidationError list option
    }

    type Response =
    | Empty
    | UserRegistered of StreamId * Claim list
    | UserLoggedIn of StreamId * Claim list
    | UserVerified of StreamId * Claim list
    | PasswordChanged of StreamId
    | RequestedPasswordReset of StreamId
    | VerifiedPasswordReset of StreamId * Claim list

    type Error =
    // Validation errors
    | EmailIsRequired
    | EmailMustBeValid of string

    | PasswordIsRequired
    | PasswordIsTooShort of int
    | PasswordMustBeValid

    | TokenIsRequired
    | TokenMustBeValid of string

    // State errors
    | UserAlreadyExists
    | UserNotVerified
    | UserDoesNotExist
    | AuthenticationFailed
    | VerificationFailed
    | InvalidState of string
    | InvalidStateTransition of string

    // Exceptions
    | SaveVersionException of exn
    | SaveException of exn
    | InternalException of exn

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

    type User =
    | Init
    | UnverifiedUser of UserInfo * VerificationToken
    | VerifiedUser of UserInfo * PasswordResetInfo option
    | Deleted

    let buildError errorMessage =
        { Message = errorMessage; Inner = None }

    let formatErrors errors =
        let rec formatError error =
            match error with
            | EmailIsRequired -> sprintf "Email is required."
            | EmailMustBeValid email -> sprintf "Invalid email '%s'." email

            | PasswordIsRequired -> sprintf "Password is required."
            | PasswordIsTooShort minimumLength -> sprintf "Password must be at least %i characters long." minimumLength
            | PasswordMustBeValid -> sprintf "Invalid password"

            | TokenIsRequired -> sprintf "Token is required."
            | TokenMustBeValid token -> sprintf "Invalid token '%s'." token

            | UserAlreadyExists -> sprintf "User already exists."
            | UserNotVerified -> sprintf "User is not verified."
            | UserDoesNotExist -> sprintf "User does not exist."
            | AuthenticationFailed -> sprintf "Authentication failed."
            | VerificationFailed -> sprintf "Verification failed."

            | InvalidState user
                -> sprintf "Trying to do an invalid operation for %s" user // TODO: Log ex

            | InvalidStateTransition _
            | SaveVersionException _
            | SaveException _
            | InternalException _
                -> sprintf "Something went wrong internally." // TODO: Log ex

            |> buildError

        errors
        |> List.map formatError

[<AutoOpen>]
module TypeCreators =
    open Exira.ErrorHandling

    let createEmail email =
        let map = function
        | StringError.Missing -> EmailIsRequired
        | _ -> EmailMustBeValid email

        email
        |> construct Email.createWithCont
        |> mapMessages map

    let createPassword password =
        let map = function
        | StringError.Missing -> PasswordIsRequired
        | TooShort minimumLength -> PasswordIsTooShort minimumLength
        | _ -> PasswordMustBeValid

        password
        |> construct Password.createWithCont
        |> mapMessages map

    let createToken token =
        let map = function
        | StringError.Missing -> TokenIsRequired
        | _ -> TokenMustBeValid token

        token
        |> construct Token.createWithCont
        |> mapMessages map

//    let createRole role =
//        let map = function
//        | StringError.Missing -> RoleIsRequired
//        | DoesntMatchPattern validRoles -> RoleMustBeValid (role, validRoles)
//
//        role
//        |> construct Role.createWithCont
//        |> mapMessages map
