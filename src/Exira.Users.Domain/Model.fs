namespace Exira.Users.Domain

[<AutoOpen>]
module Model =
    open System

    /// An account has a unique name, an email and a one or more users belonging to it
    type AccountInfo = {
        Name: AccountName.AccountName
        Email: EmailInfo.EmailInfo
        Users: Email.Email list
    }

    /// A user has a unique email, a password, some roles a personal account and belongs to zero or more accounts
    and UserInfo = {
        Email: EmailInfo.EmailInfo
        Hash: PasswordHash.PasswordHash
        Roles: Role.Role list
        PersonalAccount: AccountName.AccountName
        Accounts: AccountName.AccountName list
    }

    type VerificationToken = VerificationToken of Token.Token
    type PasswordResetToken = PasswordResetToken of Token.Token

    type PasswordResetInfo = {
        RequestedAt: DateTime
        ResetToken: PasswordResetToken
    }

    type PersonalAccount = { Account: AccountInfo }
    type CompanyAccount = { Account: AccountInfo }
    type Account =
    | Init
    | PersonalAccount of PersonalAccount
    | CompanyAccount of CompanyAccount
    | Deleted

    type UnverifiedUser = { User: UserInfo; VerificationToken: VerificationToken }
    type VerifiedUser = { User: UserInfo; PasswordResetInfo: PasswordResetInfo option }
    type User =
    | Init
    | UnverifiedUser of UnverifiedUser
    | VerifiedUser of VerifiedUser
    | Deleted

[<AutoOpen>]
module RailwayResults =
    open System.Security.Claims
    open Exira.EventStore

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

[<AutoOpen>]
module ErrorHelpers =
    open Newtonsoft.Json

    type ValidationError = {
        [<JsonProperty(PropertyName = "field")>]
        Field: string option;
        [<JsonProperty(PropertyName = "error")>]
        Message: string;
        Inner: ValidationError list option
    }

    let buildError (errorField, errorMessage) =
        { Field = errorField; Message = errorMessage; Inner = None }

    let formatErrors errors =
        let rec formatError error =
            match error with
            | EmailIsRequired -> (Some "email", "Email is required.")
            | EmailMustBeValid email -> (Some "email", sprintf "Invalid email '%s'." email)

            | PasswordIsRequired -> (Some "password", "Password is required.")
            | PasswordIsTooShort minimumLength -> (Some "password", sprintf "Password must be at least %i characters long." minimumLength)
            | PasswordMustBeValid -> (Some "password", "Invalid password")

            | TokenIsRequired -> (Some "token", "Token is required.")
            | TokenMustBeValid token -> (Some "token", sprintf "Invalid token '%s'." token)

            | UserAlreadyExists -> (Some "email", "User already exists.")
            | UserNotVerified -> (None, "User is not verified.")
            | UserDoesNotExist -> (None, "User does not exist.")
            | AuthenticationFailed -> (None, "Authentication failed.")
            | VerificationFailed -> (None, "Verification failed.")

            | InvalidState user
                -> (None, sprintf "Trying to do an invalid operation for %s" user) // TODO: Log ex

            | InvalidStateTransition _
            | SaveVersionException _
            | SaveException _
            | InternalException _
                -> (None, "Something went wrong internally.") // TODO: Log ex

            |> buildError

        errors
        |> List.map formatError