namespace Exira.Users.Domain

open Exira.ErrorHandling

module User =
    open Events

    let internal toPersonalAccount email =
        email
        |> Email.value
        |> sprintf "%s-%s" AccountName.PersonalAccountNamePrefix
        |> AccountName.create
        |> Option.get

    let internal applyUserEvent state event =
        match state with
        | User.Init ->
            match event with
            | UserRegistered e ->
                let email =
                    match e.Email with
                    | UnverifiedEmail email
                    | VerifiedEmail (email, _) -> email

                let personalAccount = toPersonalAccount email

                UnverifiedUser {
                    User =
                        { Email = e.Email
                          Hash = e.Hash
                          Roles = e.Roles
                          PersonalAccount = personalAccount
                          Accounts = [] }
                    VerificationToken = e.VerificationToken } |> Success

            | UserLoggedIn _
            | UserVerified _
            | PasswordChanged _
            | RequestedPasswordReset _
            | VerifiedPasswordReset _ -> stateTransitionFail event state

        | User.UnverifiedUser user ->
            match event with
            | UserLoggedIn _ -> state |> Success
            | UserVerified e ->
                VerifiedUser {
                    User = { user.User with Email = e.Email }
                    PasswordResetInfo = None } |> Success

            | UserRegistered _
            | PasswordChanged _
            | RequestedPasswordReset _
            | VerifiedPasswordReset _ -> stateTransitionFail event state

        | User.VerifiedUser user ->
            match event, user.PasswordResetInfo with
            // Regular login, nothing special
            | UserLoggedIn _, None -> state |> Success

            // Logging in but has a password reset pending, user obviously remembered password, get rid of reset
            | UserLoggedIn _, Some _ -> VerifiedUser { User = user.User; PasswordResetInfo = None } |> Success

            // User changed his password, reset or not, we get rid of it
            | PasswordChanged e, _ -> VerifiedUser { User = { user.User with Hash = e.Hash }; PasswordResetInfo = None } |> Success

            // User requested a password reset, store it
            | RequestedPasswordReset e, _ -> VerifiedUser { User = user.User; PasswordResetInfo = Some { RequestedAt = e.RequestedAt; ResetToken = e.ResetToken } } |> Success

            // The password reset was successful, get rid of it
            | VerifiedPasswordReset e, _ -> VerifiedUser { User = { user.User with Hash = e.Hash }; PasswordResetInfo = None } |> Success

            | UserRegistered _, _
            | UserVerified _, _ -> stateTransitionFail event state

        | User.Deleted ->
            match event with
            | _ -> stateTransitionFail event state

    let internal applyEvent state event =
        match event with
        | Event.User e -> applyUserEvent state e
        | _ -> stateTransitionFail event state

    let getUserState id = getState (applyEvents applyEvent) User.Init (toUserStreamId id)

[<AutoOpen>]
module internal UserCommandHandler =
    open System
    open System.Security.Claims
    open EventStore.ClientAPI
    open Commands
    open Events
    open User

    let [<Literal>] VerificationTokenLength = 40
    let [<Literal>] PasswordResetTokenLength = 40
    let [<Literal>] PasswordResetTokenDurationInMinutes = 120.0

    let buildClaims roles email accounts =
        let roles = roles |> List.map (fun role -> Claim(ClaimTypes.Role, (Role.value role).toString))

        let personalAccount = toPersonalAccount email
        let accounts =
            accounts
            |> List.map (fun account -> Claim("Account", AccountName.value account))
            |> List.append [Claim("Account", AccountName.value personalAccount)]

        let basic = [
            Claim(ClaimTypes.AuthenticationInstant, DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"))
            Claim(ClaimTypes.AuthenticationMethod, AuthenticationTypes.Password)
            Claim(ClaimTypes.Name, Email.value email)
            Claim("MainAccount", AccountName.value personalAccount) ]

        basic
        |> List.append roles
        |> List.append accounts

    let createUser (command: RegisterCommand) (_, state) =
        // A user can only be created when it does not exist yet
        match state with
        | User.Init ->
            let unverifiedEmail = EmailInfo.create command.Email
            let randomToken = String.random VerificationTokenLength
            let token = Token.create randomToken |> Option.get |> VerificationToken
            let role = RoleType.User |> Role

            // TODO: Dont forget to also create the personal account afterwards!

            let userCreated =
                { UserRegisteredEvent.Email = unverifiedEmail
                  VerificationToken = token
                  Hash = createHash command.Password
                  Roles = [ role ] } |> UserRegistered |> Event.User

            let streamId = toUserStreamId command.Email
            let claims = buildClaims [role] command.Email []
            let response = Response.UserRegistered (streamId, claims)
            succeed (streamId, ExpectedVersion.NoStream, [userCreated], response)

        | User.UnverifiedUser _
        | User.VerifiedUser _
        | User.Deleted _ -> fail [UserAlreadyExists]

    let loginUser (command: LoginCommand) (_, state) =
        // A user can only login when it exists and the password is correct
        match state with
        | User.UnverifiedUser { User = user }
        | User.VerifiedUser { User = user } when validatePassword command.Password user.Hash ->
            let userLoggedIn =
                { UserLoggedInEvent.LoggedInAt = DateTime.UtcNow } |> UserLoggedIn |> Event.User

            let streamId = toUserStreamId command.Email
            let claims = buildClaims user.Roles command.Email user.Accounts
            let response = Response.UserLoggedIn (streamId, claims)
            succeed ((toUserStreamId command.Email), ExpectedVersion.Any, [userLoggedIn], response)

        | User.Init
        | User.UnverifiedUser _
        | User.VerifiedUser _
        | User.Deleted _ ->
            // TODO: Sleep a bit here to prevent timing attacks
            fail [AuthenticationFailed]

    let verifyUser (command: VerifyCommand) (_, state) =
        // A user can only be verified if it exists, is not verified yet and supplied the correct token
        match state with
        | User.UnverifiedUser { User = user; VerificationToken = token } when command.Token = token ->
            let verifiedEmail = EmailInfo.verified user.Email DateTime.UtcNow

            // TODO: Dont forget to also verify the personal account

            let userVerified =
                 { UserVerifiedEvent.Email = verifiedEmail } |> UserVerified |> Event.User

            let streamId = toUserStreamId command.Email
            let claims = buildClaims user.Roles command.Email user.Accounts
            let response = Response.UserVerified (streamId, claims)
            succeed (streamId, ExpectedVersion.Any, [userVerified], response)

        | User.Init
        | User.UnverifiedUser _
        | User.VerifiedUser _
        | User.Deleted _ -> fail [VerificationFailed]

    let changePassword (command: ChangePasswordCommand) (_, state) =
        // A user can only change password when it exists, is verified and supplied the correct old password
        match state with
        | User.VerifiedUser { User = user } when validatePassword command.PreviousPassword user.Hash ->
            let passwordChanged =
                { PasswordChangedEvent.Hash = createHash command.NewPassword } |> PasswordChanged |> Event.User

            let streamId = toUserStreamId command.Email
            let response = Response.PasswordChanged streamId
            succeed (streamId, ExpectedVersion.Any, [passwordChanged], response)

        | User.Init
        | User.Deleted _ -> fail [UserDoesNotExist]
        | User.UnverifiedUser _ -> fail [UserNotVerified]
        | User.VerifiedUser _ -> fail [AuthenticationFailed]

    let requestPasswordReset (command: RequestPasswordResetCommand) (_, state) =
        // A user can only request a password reset when it exists and is verified
        match state with
        | User.VerifiedUser _  ->
            let randomToken = String.random PasswordResetTokenLength
            let token = Token.create randomToken |> Option.get

            let requestedPasswordReset =
                { RequestedPasswordResetEvent.RequestedAt = DateTime.UtcNow
                  ResetToken = PasswordResetToken token } |> RequestedPasswordReset |> Event.User

            let streamId = toUserStreamId command.Email
            let response = Response.RequestedPasswordReset streamId
            succeed (streamId, ExpectedVersion.Any, [requestedPasswordReset], response)

        | User.Init
        | User.Deleted _ -> fail [UserDoesNotExist]
        | User.UnverifiedUser _ -> fail [UserNotVerified]

    let verifyPasswordReset (command: VerifyPasswordResetCommand) (_, state) =
        let validatePasswordResetInfo token (resetInfo: PasswordResetInfo) =
            // Check if the token has not expired yet, and is identical
            let resetActiveUntil = resetInfo.RequestedAt.AddMinutes PasswordResetTokenDurationInMinutes
            resetInfo.ResetToken = token && resetActiveUntil > DateTime.UtcNow

        // A user can only reset a password when it exists and matches the token inside the time window
        match state with
        | User.VerifiedUser { User = user; PasswordResetInfo = Some resetInfo } when validatePasswordResetInfo command.Token resetInfo ->
            let verifiedPasswordReset =
                { VerifiedPasswordResetEvent.VerifiedAt = DateTime.UtcNow
                  Hash = createHash command.NewPassword } |> VerifiedPasswordReset |> Event.User

            let streamId = toUserStreamId command.Email
            let claims = buildClaims user.Roles command.Email user.Accounts
            let response = Response.VerifiedPasswordReset (streamId, claims)
            succeed (streamId, ExpectedVersion.Any, [verifiedPasswordReset], response)

        | User.Init
        | User.Deleted _ -> fail [UserDoesNotExist]
        | User.UnverifiedUser _ -> fail [UserNotVerified]
        | User.VerifiedUser _ -> fail [VerificationFailed]

    let handleRegister (command: RegisterCommand) es =
        async {
            let! state = getUserState command.Email es

            return!
                state
                |> bind (createUser command)
                |> bindAsync (save es)
        }

    let handleLogin (command: LoginCommand) es =
        async {
            let! state = getUserState command.Email es

            return!
                state
                |> bind (loginUser command)
                |> bindAsync (save es)
        }

    let handleVerify (command: VerifyCommand) es =
        async {
            let! state = getUserState command.Email es

            return!
                state
                |> bind (verifyUser command)
                |> bindAsync (save es)
        }

    let handleChangePassword (command: ChangePasswordCommand) es =
        async {
            let! state = getUserState command.Email es

            return!
                state
                |> bind (changePassword command)
                |> bindAsync (save es)
        }

    let handleRequestPasswordReset (command: RequestPasswordResetCommand) es =
        async {
            let! state = getUserState command.Email es

            return!
                state
                |> bind (requestPasswordReset command)
                |> bindAsync (save es)
        }

    let handleVerifyPasswordReset (command: VerifyPasswordResetCommand) es =
        async {
            let! state = getUserState command.Email es

            return!
                state
                |> bind (verifyPasswordReset command)
                |> bindAsync (save es)
        }
