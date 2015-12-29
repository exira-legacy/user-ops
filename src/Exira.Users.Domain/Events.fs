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
            | User user -> Json.writeWith Json.serialize "user" user
            | Account account -> Json.writeWith Json.serialize "account" account

        static member FromJson (_: Event) = function
            | Property "user" x as json -> Json.init (User x) json
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
            | UserRegistered userRegistered -> Json.writeWith Json.serialize "userRegistered" userRegistered
            //| UserLoggedIn ev -> writeUnion "userLoggedIn" ev
            //| UserVerified ev -> writeUnion "userVerified" ev
            //| PasswordChanged ev -> writeUnion "passwordChanged" ev
            //| RequestedPasswordReset ev -> writeUnion "requestedPasswordReset" ev
            //| VerifiedPasswordReset ev -> writeUnion "verifiedPasswordReset" ev

        static member FromJson (_: UserEvent) = function
            | Property "userRegistered" x as json -> Json.init (UserRegistered x) json
            | json -> Json.error (sprintf "couldn't deserialise %A to UserEvent" json) json

    and UserRegisteredEvent = {
        Email: EmailInfo.EmailInfo
        VerificationToken: VerificationToken
        Hash: PasswordHash.PasswordHash
        Roles: Role.Role list
    } with
        static member ToJson (e: UserRegisteredEvent) =
            let (VerificationToken token) = e.VerificationToken

            Json.writeWith EmailInfo.toJson "emailInfo" e.Email
            *> Json.writeWith Token.toJson "token" token
            *> Json.writeWith PasswordHash.toJson "hash" e.Hash
            *> Json.writeWith Role.toJson "roles" e.Roles

        static member FromJson (_: UserRegisteredEvent) =
            (fun emailInfo token hash roles ->
                { Email = emailInfo
                  VerificationToken = VerificationToken token
                  Hash = hash
                  Roles = roles })
            <!> Json.readWith EmailInfo.fromJson "emailInfo"
            <*> Json.readWith Token.fromJson "token"
            <*> Json.readWith PasswordHash.fromJson "hash"
            <*> Json.readWith Role.fromJson "roles"

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

    and AccountEvent =
    | AccountCreated of AccountCreatedEvent
    with
        static member ToJson (e: AccountEvent) =
            match e with
            | AccountCreated _ -> ToJsonDefaults.ToJson "accountCreated"

    and AccountCreatedEvent = {
        Type: AccountType
        Name: AccountName.AccountName
        Email: EmailInfo.EmailInfo
        Users: Email.Email list
    }