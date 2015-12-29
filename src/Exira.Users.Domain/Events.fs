namespace Exira.Users.Domain

module Events =
    open System
    open Chiron
    open Chiron.Operators

    type Event =
    | User of UserEvent
    | Account of AccountEvent
    with
        static member ToJson (e: Event) =
            match e with
            | User user -> Json.write "user" user
            | Account account -> Json.write "account" account

        static member FromJson (_: Event) = function
            | Property "user" x as json -> Json.init (User x) json
            | Property "account" x as json -> Json.init (Account x) json
            | json -> Json.error (sprintf "couldn't deserialise %A to Event" json) json

    and UserEvent =
    | UserRegistered of UserRegisteredEvent
    | UserLoggedIn of UserLoggedInEvent
    | UserVerified of UserVerifiedEvent
    | PasswordChanged of PasswordChangedEvent
    | RequestedPasswordReset of RequestedPasswordResetEvent
    | VerifiedPasswordReset of VerifiedPasswordResetEvent
    with
        static member ToJson (e: UserEvent) =
            match e with
            | UserRegistered userRegistered -> Json.write "userRegistered" userRegistered
            | UserLoggedIn userLoggedIn -> Json.write "userLoggedIn" userLoggedIn
            | UserVerified userVerified -> Json.write "userVerified" userVerified
            | PasswordChanged passwordChanged -> Json.write "passwordChanged" passwordChanged
            | RequestedPasswordReset requestedPasswordReset -> Json.write "requestedPasswordReset" requestedPasswordReset
            | VerifiedPasswordReset verifiedPasswordReset -> Json.write "verifiedPasswordReset" verifiedPasswordReset

        static member FromJson (_: UserEvent) = function
            | Property "userRegistered" x as json -> Json.init (UserRegistered x) json
            | Property "userLoggedIn" x as json -> Json.init (UserLoggedIn x) json
            | Property "userVerified" x as json -> Json.init (UserVerified x) json
            | Property "passwordChanged" x as json -> Json.init (PasswordChanged x) json
            | Property "requestedPasswordReset" x as json -> Json.init (RequestedPasswordReset x) json
            | Property "verifiedPasswordReset" x as json -> Json.init (VerifiedPasswordReset x) json
            | json -> Json.error (sprintf "couldn't deserialise %A to UserEvent" json) json

    and UserRegisteredEvent = {
        Email: EmailInfo.EmailInfo
        VerificationToken: VerificationToken
        Hash: PasswordHash.PasswordHash
        Roles: Role.Role list
    } with
        static member ToJson (e: UserRegisteredEvent) =
            let (VerificationToken token) = e.VerificationToken

            Json.write "emailInfo" e.Email
            *> Json.write "token" token
            *> Json.write "hash" e.Hash
            *> Json.write "roles" e.Roles

        static member FromJson (_: UserRegisteredEvent) =
            (fun emailInfo token hash roles ->
                { Email = emailInfo
                  VerificationToken = VerificationToken token
                  Hash = hash
                  Roles = roles })
            <!> Json.read "emailInfo"
            <*> Json.read "token"
            <*> Json.read "hash"
            <*> Json.read "roles"

    and UserLoggedInEvent = {
        LoggedInAt: DateTime
    } with
        static member ToJson (e: UserLoggedInEvent) =
            Json.write "loggedInAt" e.LoggedInAt

        static member FromJson (_: UserLoggedInEvent) =
            (fun loggedInAt -> { LoggedInAt = loggedInAt })
            <!> Json.read "loggedInAt"

    and UserVerifiedEvent = {
        Email: EmailInfo.EmailInfo
    } with
        static member ToJson (e: UserVerifiedEvent) =
            Json.write "emailInfo" e.Email

        static member FromJson (_: UserVerifiedEvent) =
            (fun emailInfo -> { Email = emailInfo })
            <!> Json.read "emailInfo"

    and PasswordChangedEvent = {
        Hash: PasswordHash.PasswordHash
    } with
        static member ToJson (e: PasswordChangedEvent) =
            Json.write "hash" e.Hash

        static member FromJson (_: PasswordChangedEvent) =
            (fun hash -> { Hash = hash })
            <!> Json.read "hash"

    and RequestedPasswordResetEvent = {
        RequestedAt: DateTime
        ResetToken: PasswordResetToken
    } with
        static member ToJson (e: RequestedPasswordResetEvent) =
            let (PasswordResetToken token) = e.ResetToken

            Json.write "requestedAt" e.RequestedAt
            *> Json.write "token" token

        static member FromJson (_: RequestedPasswordResetEvent) =
            (fun requestedAt token -> { RequestedAt = requestedAt; ResetToken = PasswordResetToken token })
            <!> Json.read "requestedAt"
            <*> Json.read "token"

    and VerifiedPasswordResetEvent = {
        VerifiedAt: DateTime
        Hash: PasswordHash.PasswordHash
    } with
        static member ToJson (e: VerifiedPasswordResetEvent) =
            Json.write "verifiedAt" e.VerifiedAt
            *> Json.write "hash" e.Hash

        static member FromJson (_: VerifiedPasswordResetEvent) =
            (fun verifiedAt hash -> { VerifiedAt = verifiedAt; Hash = hash })
            <!> Json.read "verifiedAt"
            <*> Json.read "hash"

    and AccountEvent =
    | AccountCreated of AccountCreatedEvent
    with
        static member ToJson (e: AccountEvent) =
            match e with
            | AccountCreated accountCreated -> Json.write "accountCreated" accountCreated

        static member FromJson (_: AccountEvent) = function
            | Property "accountCreated" x as json -> Json.init (AccountCreated x) json
            | json -> Json.error (sprintf "couldn't deserialise %A to AccountEvent" json) json

    and AccountCreatedEvent = {
        Type: AccountType
        Name: AccountName.AccountName
        Email: EmailInfo.EmailInfo
        Users: Email.Email list
    } with
        static member ToJson (e: AccountCreatedEvent) =
            Json.write "type" e.Type
            *> Json.write "name" e.Name
            *> Json.write "email" e.Email
            *> Json.write "users" e.Users

        static member FromJson (_: AccountCreatedEvent) =
            (fun t name email users ->
                { Type = t
                  Name = name
                  Email = email
                  Users = users })
            <!> Json.read "type"
            <*> Json.read "name"
            <*> Json.read "email"
            <*> Json.read "users"