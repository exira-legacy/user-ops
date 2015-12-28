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
            | User user -> Json.write "type" "user" *> Json.writeWith Json.serialize "user" user
            | Account account -> Json.write "type" "account" *> Json.writeWith Json.serialize "account" account

        static member FromJson (_: Event) =
            json {
                let! t = Json.read "type"

                return!
                    match t with
                    | "user" -> User << Json.deserialize <!> Json.read "user"
                    //| "account" -> Account << Json.deserialize <!> Json.read "account"
                    | _ -> Json.error (sprintf "Expected Event")
            }

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
            | UserRegistered e -> Json.write "type" "userRegistered" *> Json.writeWith Json.serialize "userRegistered" e
            | UserLoggedIn _ -> Json.write "type" "userLoggedIn" *> ToJsonDefaults.ToJson "userLoggedIn"
            | UserVerified _ -> Json.write "type" "userVerified" *> ToJsonDefaults.ToJson "userVerified"
            | PasswordChanged _ -> Json.write "type" "passwordChanged" *> ToJsonDefaults.ToJson "passwordChanged"
            | RequestedPasswordReset _ -> Json.write "type" "requestedPasswordReset" *> ToJsonDefaults.ToJson "requestedPasswordReset"
            | VerifiedPasswordReset _ -> Json.write "type" "verifiedPasswordReset" *> ToJsonDefaults.ToJson "verifiedPasswordReset"

        static member FromJson (_: UserEvent) =
            json {
                let! t = Json.read "type"

                return!
                    match t with
                    | "userRegistered" -> UserRegistered << Json.deserialize <!> Json.read "userRegistered"
                    //TODO: | _
            }

    and UserRegisteredEvent = {
        Email: EmailInfo.EmailInfo
        VerificationToken: VerificationToken
        Hash: PasswordHash.PasswordHash
        Roles: Role.Role list
    } with
        static member ToJson (e: UserRegisteredEvent) =
            Json.writeWith EmailInfo.toJson "emailInfo" e.Email

        static member FromJson (_: UserRegisteredEvent) =
            // TODO: Get rid of this temp
            fun x -> { Email = x
                       VerificationToken = VerificationToken (Token.Token ("bla"))
                       Hash = PasswordHash ("test")
                       Roles = [] }
            <!> Json.readWith EmailInfo.fromJson "emailInfo"

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