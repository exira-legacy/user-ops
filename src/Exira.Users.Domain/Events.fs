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
            | UserRegistered userRegistered -> Json.writeWith Json.serialize "userRegistered" userRegistered
            | UserLoggedIn userLoggedIn -> Json.writeWith Json.serialize "userLoggedIn" userLoggedIn
            | UserVerified userVerified -> Json.writeWith Json.serialize "userVerified" userVerified
            | PasswordChanged passwordChanged -> Json.writeWith Json.serialize "passwordChanged" passwordChanged
            | RequestedPasswordReset requestedPasswordReset -> Json.writeWith Json.serialize "requestedPasswordReset" requestedPasswordReset
            | VerifiedPasswordReset verifiedPasswordReset -> Json.writeWith Json.serialize "verifiedPasswordReset" verifiedPasswordReset

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
    } with
        static member ToJson (e: UserLoggedInEvent) =
            Json.write "loggedInAt" e.LoggedInAt // TODO: Check if we need to ToString("o") this

        static member FromJson (_: UserLoggedInEvent) =
            (fun loggedInAt -> { LoggedInAt = loggedInAt })
            <!> Json.read "loggedInAt"

    and UserVerifiedEvent = {
        Email: EmailInfo.EmailInfo
    } with
        static member ToJson (e: UserVerifiedEvent) =
            Json.writeWith EmailInfo.toJson "emailInfo" e.Email

        static member FromJson (_: UserVerifiedEvent) =
            (fun emailInfo -> { Email = emailInfo })
            <!> Json.readWith EmailInfo.fromJson "emailInfo"

    and PasswordChangedEvent = {
        Hash: PasswordHash.PasswordHash
    } with
        static member ToJson (e: PasswordChangedEvent) =
            Json.writeWith PasswordHash.toJson "hash" e.Hash

        static member FromJson (_: PasswordChangedEvent) =
            (fun hash -> { Hash = hash })
            <!> Json.readWith PasswordHash.fromJson "hash"

    and RequestedPasswordResetEvent = {
        RequestedAt: DateTime
        ResetToken: PasswordResetToken
    } with
        static member ToJson (e: RequestedPasswordResetEvent) =
            let (PasswordResetToken token) = e.ResetToken

            Json.write "requestedAt" e.RequestedAt // TODO: Check if we need to ToString("o") this
            *> Json.writeWith Token.toJson "token" token

        static member FromJson (_: RequestedPasswordResetEvent) =
            (fun requestedAt token -> { RequestedAt = requestedAt; ResetToken = PasswordResetToken token })
            <!> Json.read "requestedAt"
            <*> Json.readWith Token.fromJson "token"

    and VerifiedPasswordResetEvent = {
        VerifiedAt: DateTime
        Hash: PasswordHash.PasswordHash
    } with
        static member ToJson (e: VerifiedPasswordResetEvent) =
            Json.write "verifiedAt" e.VerifiedAt // TODO: Check if we need to ToString("o") this
            *> Json.writeWith PasswordHash.toJson "hash" e.Hash

        static member FromJson (_: VerifiedPasswordResetEvent) =
            (fun verifiedAt hash -> { VerifiedAt = verifiedAt; Hash = hash })
            <!> Json.read "verifiedAt"
            <*> Json.readWith PasswordHash.fromJson "hash"

    and AccountEvent =
    | AccountCreated of AccountCreatedEvent
    with
        static member ToJson (e: AccountEvent) =
            match e with
            | AccountCreated accountCreated -> Json.writeWith Json.serialize "accountCreated" accountCreated

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
            let t =
                match e.Type with
                | Personal -> "personal"
                | Company -> "company"

            Json.write "type" t
            *> Json.writeWith AccountName.toJson "name" e.Name
            *> Json.writeWith EmailInfo.toJson "email" e.Email
            *> Json.writeWith Email.multipleToJson "users" e.Users

        static member FromJson (_: AccountCreatedEvent) =
            (fun t name email users ->
                let t =
                    match t with
                    | "personal" -> Personal
                    | "company" -> Company
                    | _ -> failwithf "couldn't deserialise %s to AccountType" t

                { Type = t
                  Name = name
                  Email = email
                  Users = users })
            <!> Json.read "type"
            <*> Json.readWith AccountName.fromJson "name"
            <*> Json.readWith EmailInfo.fromJson "email"
            <*> Json.readWith Email.multipleFromJson "users"