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

    let private applyUserEvent state event =
        match state with
        | User.Init ->
            match event with
            | UserRegistered e ->
                let email =
                    match e.Email with
                    | UnverifiedEmail (Email = email)
                    | VerifiedEmail (Email = email) -> email

                let personalAccount = toPersonalAccount email

                UnverifiedUser (VerificationToken = e.VerificationToken,
                    User = {
                        Email = e.Email
                        Hash = e.Hash
                        Roles = e.Roles
                        PersonalAccount = personalAccount
                        Accounts = [] }) |> Success

            | UserLoggedIn _
            | UserVerified _
            | PasswordChanged _
            | RequestedPasswordReset _
            | VerifiedPasswordReset _ -> stateTransitionFail event state

        | User.UnverifiedUser (User = user) ->
            match event with
            | UserLoggedIn _ -> state |> Success
            | UserVerified e ->
                VerifiedUser (User = { user with Email = e.Email }, PasswordResetInfo = None) |> Success

            | UserRegistered _
            | PasswordChanged _
            | RequestedPasswordReset _
            | VerifiedPasswordReset _ -> stateTransitionFail event state

        | User.VerifiedUser (User = user; PasswordResetInfo = resetInfo) ->
            match event, resetInfo with
            // Regular login, nothing special
            | UserLoggedIn _, None -> state |> Success

            // Logging in but has a password reset pending, user obviously remembered password, get rid of reset
            | UserLoggedIn _, Some _ -> VerifiedUser (User = user, PasswordResetInfo = None) |> Success

            // User changed his password, reset or not, we get rid of it
            | PasswordChanged e, _ -> VerifiedUser (User = { user with Hash = e.Hash }, PasswordResetInfo = None) |> Success

            // User requested a password reset, store it
            | RequestedPasswordReset e, _ -> VerifiedUser (User = user, PasswordResetInfo = Some { RequestedAt = e.RequestedAt; ResetToken = e.ResetToken }) |> Success

            // The password reset was successful, get rid of it
            | VerifiedPasswordReset e, _ -> VerifiedUser (User = { user with Hash = e.Hash }, PasswordResetInfo = None) |> Success

            | UserRegistered _, _
            | UserVerified _, _ -> stateTransitionFail event state

        | User.Deleted ->
            match event with
            | _ -> stateTransitionFail event state

    let private applyEvent state event =
        match event with
        | Event.User e -> applyUserEvent state e
        | _ -> stateTransitionFail event state

    let getUserState id = getState (applyEvents applyEvent) User.Init (toUserStreamId id)

module internal UserCommandHandler =
    open System
    open System.Security.Claims
    open EventStore.ClientAPI
    open Commands
    open Events
    open User

    let private logger = Logging.logger

    let [<Literal>] private VerificationTokenLength = 40
    let [<Literal>] private PasswordResetTokenLength = 40
    let [<Literal>] private PasswordResetTokenDurationInMinutes = 120.0

    let private buildClaims roles email accounts =
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

    let private createUser (command: RegisterCommand) (_, state) =
        logger.Information("Creating user {Username}", command.Email)
        logger.Debug("UserCommandHandler.createUser: {@Command}, current state: {@State}", command, state)

        // A user can only be created when it does not exist yet
        match state with
        | User.Init ->
            let unverifiedEmail = EmailInfo.create command.Email
            let randomToken = String.random VerificationTokenLength
            let token = Token.create randomToken |> Option.get |> VerificationToken
            let role = RoleType.User |> Role

            let userCreated =
                { UserRegisteredEvent.Email = unverifiedEmail
                  VerificationToken = token
                  Hash = createHash command.Password
                  Roles = [ role ] } |> UserRegistered |> Event.User

            let streamId = toUserStreamId command.Email
            let claims = buildClaims [role] command.Email []
            let response = Response.UserRegistered (streamId, claims)

            logger.Information("Created user {Username}", command.Email)
            logger.Debug("Generated UserRegisteredEvent {@Event}", userCreated)
            succeed (streamId, ExpectedVersion.NoStream, [userCreated], response)

        | User.UnverifiedUser _
        | User.VerifiedUser _
        | User.Deleted _ ->
            logger.Information("Cannot create user {Username}, it already exists", command.Email)
            fail [UserAlreadyExists]

    let private loginUser (command: LoginCommand) (_, state) =
        logger.Information("Logging in user {Username}", command.Email)
        logger.Debug("UserCommandHandler.loginUser: {@Command}, current state: {@State}", command, state)

        let authenticationFailed () =
            // TODO: Sleep a bit here to prevent timing attacks
            fail [AuthenticationFailed]

        // A user can only login when it exists and the password is correct
        match state with
        | User.UnverifiedUser (User = user)
        | User.VerifiedUser (User = user) when validatePassword command.Password user.Hash ->
            let userLoggedIn =
                { UserLoggedInEvent.LoggedInAt = DateTime.UtcNow } |> UserLoggedIn |> Event.User

            let streamId = toUserStreamId command.Email
            let claims = buildClaims user.Roles command.Email user.Accounts
            let response = Response.UserLoggedIn (streamId, claims)

            logger.Information("Logged in user {Username}", command.Email)
            logger.Debug("Generated UserLoggedInEvent {@Event}", userLoggedIn)
            succeed ((toUserStreamId command.Email), ExpectedVersion.Any, [userLoggedIn], response)

        | User.Init ->
            logger.Information("Cannot login user {Username}, it does not exist", command.Email)
            authenticationFailed()

        | User.UnverifiedUser _
        | User.VerifiedUser _ ->
            logger.Information("Login information for {Username} was incorrect", command.Email)
            authenticationFailed()

        | User.Deleted _ ->
            logger.Information("Cannot login user {Username}, it is deleted", command.Email)
            authenticationFailed()

    let private verifyUser (command: VerifyCommand) (_, state) =
        logger.Information("Verifying user {Username}", command.Email)
        logger.Debug("UserCommandHandler.verifyUser: {@Command}, current state: {@State}", command, state)

        let verificationFailed () =
            fail [VerificationFailed]

        // A user can only be verified if it exists, is not verified yet and supplied the correct token
        match state with
        | User.UnverifiedUser (User = user; VerificationToken = token) when command.Token = token ->
            let verifiedEmail = EmailInfo.verified user.Email DateTime.UtcNow

            // TODO: Dont forget to also verify the personal account

            let userVerified =
                 { UserVerifiedEvent.Email = verifiedEmail } |> UserVerified |> Event.User

            let streamId = toUserStreamId command.Email
            let claims = buildClaims user.Roles command.Email user.Accounts
            let response = Response.UserVerified (streamId, claims)

            logger.Information("Verified user {Username}", command.Email)
            logger.Debug("Generated UserVerifiedEvent {@Event}", userVerified)
            succeed (streamId, ExpectedVersion.Any, [userVerified], response)

        | User.Init ->
            logger.Information("Cannot verify user {Username}, it does not exist", command.Email)
            verificationFailed()

        | User.UnverifiedUser _ ->
            logger.Information("Verification information for {Username} was incorrect", command.Email)
            verificationFailed()

        | User.VerifiedUser _ ->
            logger.Information("Cannot verify user {Username}, it is already verified", command.Email)
            verificationFailed()

        | User.Deleted _ ->
            logger.Information("Cannot verify user {Username}, it is deleted", command.Email)
            verificationFailed()

    let private changePassword (command: ChangePasswordCommand) (_, state) =
        logger.Information("Changing password for user {Username}", command.Email)
        logger.Debug("UserCommandHandler.changePassword: {@Command}, current state: {@State}", command, state)

        // A user can only change password when it exists, is verified and supplied the correct old password
        match state with
        | User.VerifiedUser (User = user) when validatePassword command.PreviousPassword user.Hash ->
            let passwordChanged =
                { PasswordChangedEvent.Hash = createHash command.NewPassword } |> PasswordChanged |> Event.User

            let streamId = toUserStreamId command.Email
            let response = Response.PasswordChanged streamId

            logger.Information("Password changed for {Username}", command.Email)
            logger.Debug("Generated PasswordChangedEvent {@Event}", passwordChanged)
            succeed (streamId, ExpectedVersion.Any, [passwordChanged], response)

        | User.Init ->
            logger.Information("Cannot change password for user {Username}, it does not exist", command.Email)
            fail [UserDoesNotExist]

        | User.Deleted _ ->
            logger.Information("Cannot change password for user {Username}, it is deleted", command.Email)
            fail [UserDoesNotExist]

        | User.UnverifiedUser _ ->
            logger.Information("Cannot request password reset for {Username}, it is not yet verified", command.Email)
            fail [UserNotVerified]

        | User.VerifiedUser _ ->
            logger.Information("Cannot request password reset for {Username}, verification information not correct", command.Email)
            fail [AuthenticationFailed]

    let private requestPasswordReset (command: RequestPasswordResetCommand) (_, state) =
        logger.Information("Requesting password reset for user {Username}", command.Email)
        logger.Debug("UserCommandHandler.requestPasswordReset: {@Command}, current state: {@State}", command, state)

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

            logger.Information("Requested password reset for {Username}", command.Email)
            logger.Debug("Generated RequestedPasswordResetEvent {@Event}", requestedPasswordReset)
            succeed (streamId, ExpectedVersion.Any, [requestedPasswordReset], response)

        | User.Init ->
            logger.Information("Cannot request password reset for user {Username}, it does not exist", command.Email)
            fail [UserDoesNotExist]

        | User.Deleted _ ->
            logger.Information("Cannot request password reset for {Username}, it is deleted", command.Email)
            fail [UserDoesNotExist]

        | User.UnverifiedUser _ ->
            logger.Information("Cannot request password reset for {Username}, it is not yet verified", command.Email)
            fail [UserNotVerified]

    let private verifyPasswordReset (command: VerifyPasswordResetCommand) (_, state) =
        logger.Information("Verifying password reset for user {Username}", command.Email)
        logger.Debug("UserCommandHandler.verifyPasswordReset: {@Command}, current state: {@State}", command, state)

        let validatePasswordResetInfo token (resetInfo: PasswordResetInfo) =
            // Check if the token has not expired yet, and is identical
            let resetActiveUntil = resetInfo.RequestedAt.AddMinutes PasswordResetTokenDurationInMinutes
            resetInfo.ResetToken = token && resetActiveUntil > DateTime.UtcNow

        // A user can only reset a password when it exists and matches the token inside the time window
        match state with
        | User.VerifiedUser (User = user; PasswordResetInfo = Some resetInfo) when validatePasswordResetInfo command.Token resetInfo ->
            let verifiedPasswordReset =
                { VerifiedPasswordResetEvent.VerifiedAt = DateTime.UtcNow
                  Hash = createHash command.NewPassword } |> VerifiedPasswordReset |> Event.User

            let streamId = toUserStreamId command.Email
            let claims = buildClaims user.Roles command.Email user.Accounts
            let response = Response.VerifiedPasswordReset (streamId, claims)

            logger.Information("Verified password reset for {Username}", command.Email)
            logger.Debug("Generated VerifiedPasswordResetEvent {@Event}", verifiedPasswordReset)
            succeed (streamId, ExpectedVersion.Any, [verifiedPasswordReset], response)

        | User.Init ->
            logger.Information("Cannot verify password reset for user {Username}, it does not exist", command.Email)
            fail [UserDoesNotExist]

        | User.Deleted _ ->
            logger.Information("Cannot verify password reset for user {Username}, it is deleted", command.Email)
            fail [UserDoesNotExist]

        | User.UnverifiedUser _ ->
            logger.Information("Cannot verify password reset for user {Username}, it is not yet verified", command.Email)
            fail [UserNotVerified]

        | User.VerifiedUser _ ->
            logger.Information("Cannot verify password reset for user {Username}, verification info not correct", command.Email)
            fail [VerificationFailed]

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
