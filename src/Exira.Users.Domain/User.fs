namespace Exira.Users.Domain

open Exira.ErrorHandling
open Exira.EventStore.EventStore

[<AutoOpen>]
module internal User =
    let applyUserEvent state event =
        match state with
        | Init ->
            match event with
            | UserRegistered e ->
                let user = { UserInfo.Email = e.Email; Hash = e.Hash; Roles = e.Roles }, e.VerificationToken
                UnverifiedUser user |> Success

            | UserLoggedIn _
            | UserVerified _
            | PasswordChanged _
            | RequestedPasswordReset _
            | VerifiedPasswordReset _ -> stateTransitionFail event state

        | UnverifiedUser (user, _) ->
            match event with
            | UserLoggedIn _ -> state |> Success
            | UserVerified e ->
                VerifiedUser ({ user with Email = e.Email }, None) |> Success

            | UserRegistered _
            | PasswordChanged _
            | RequestedPasswordReset _
            | VerifiedPasswordReset _ -> stateTransitionFail event state

        | VerifiedUser (user, reset) ->
            match event, reset with
            // Regular login, nothing special
            | UserLoggedIn _, None -> state |> Success

            // Logging in but has a password reset pending, user obviously remembered password, get rid of reset
            | UserLoggedIn _, Some _ -> VerifiedUser (user, None) |> Success

            // User changed his password, reset or not, we get rid of it
            | PasswordChanged e, _ -> VerifiedUser ({ user with Hash = e.Hash }, None) |> Success

            // User requested a password reset, store it
            | RequestedPasswordReset e, _ -> VerifiedUser (user, Some { RequestedAt = e.RequestedAt; ResetToken = e.ResetToken }) |> Success

            // The password reset was successful, get rid of it
            | VerifiedPasswordReset e, _ -> VerifiedUser ({ user with Hash = e.Hash }, None) |> Success

            | UserRegistered _, _
            | UserVerified _, _ -> stateTransitionFail event state

        | Deleted ->
            match event with
            | _ -> stateTransitionFail event state

    let toUserStreamId id = id |> Email.value |> toStreamId "user"
    let getUserState id = getState (applyEvents applyUserEvent) Init (toUserStreamId id)

[<AutoOpen>]
module internal UserCommandHandler =
    open System
    open System.Security.Claims
    open EventStore.ClientAPI

    let [<Literal>] VerificationTokenLength = 40
    let [<Literal>] PasswordResetTokenLength = 40
    let [<Literal>] PasswordResetTokenDurationInMinutes = 120.0

    let buildClaims roles email =
        let roles = roles |> List.map (fun role -> Claim(ClaimTypes.Role, (Role.value role).toString))
        [
            Claim(ClaimTypes.AuthenticationInstant, DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"))
            Claim(ClaimTypes.AuthenticationMethod, AuthenticationTypes.Password)
            Claim(ClaimTypes.Name, Email.value email)
        ] @ roles

    let createUser (command: RegisterCommand) (_, state) =
        // A user can only be created when it does not exist yet
        match state with
        | Init ->
            let unverifiedEmail = EmailInfo.create command.Email
            let randomToken = String.random VerificationTokenLength
            let token = Token.create randomToken |> Option.get |> VerificationToken
            let role = RoleType.User |> Role

            let userCreated = UserRegistered {
                UserRegisteredEvent.Email = unverifiedEmail
                VerificationToken = token
                Hash = createHash command.Password
                Roles = [ role ]
            }

            let streamId = toUserStreamId command.Email
            let claims = buildClaims [role] command.Email
            let response = Response.UserRegistered (streamId, claims)
            succeed (streamId, ExpectedVersion.NoStream, [userCreated], response)

        | UnverifiedUser _
        | VerifiedUser _
        | Deleted _ -> fail [UserAlreadyExists]

    let loginUser (command: LoginCommand) (_, state) =
        // A user can only login when it exists and the password is correct
        match state with
        | UnverifiedUser (u, _)
        | VerifiedUser (u, _) when validatePassword command.Password u.Hash ->
            let userLoggedIn = UserLoggedIn { UserLoggedInEvent.LoggedInAt = DateTime.UtcNow }

            let streamId = toUserStreamId command.Email
            let claims = buildClaims u.Roles command.Email
            let response = Response.UserLoggedIn (streamId, claims)
            succeed ((toUserStreamId command.Email), ExpectedVersion.Any, [userLoggedIn], response)

        | Init
        | UnverifiedUser _
        | VerifiedUser _
        | Deleted _ ->
            // TODO: Sleep a bit here to prevent timing attacks
            fail [AuthenticationFailed]

    let verifyUser (command: VerifyCommand) (_, state) =
        // A user can only be verified if it exists, is not verified yet and supplied the correct token
        match state with
        | UnverifiedUser (u, token) when command.Token = token ->
            let verifiedEmail = EmailInfo.verified u.Email DateTime.UtcNow

            let userVerified = UserVerified { UserVerifiedEvent.Email = verifiedEmail }

            let streamId = toUserStreamId command.Email
            let claims = buildClaims u.Roles command.Email
            let response = Response.UserVerified (streamId, claims)
            succeed (streamId, ExpectedVersion.Any, [userVerified], response)

        | Init
        | UnverifiedUser _
        | VerifiedUser _
        | Deleted _ -> fail [VerificationFailed]

    let changePassword (command: ChangePasswordCommand) (_, state) =
        // A user can only change password when it exists, is verified and supplied the correct old password
        match state with
        | VerifiedUser (u, _) when validatePassword command.PreviousPassword u.Hash ->
            let passwordChanged = PasswordChanged { PasswordChangedEvent.Hash = createHash command.NewPassword }

            let streamId = toUserStreamId command.Email
            let response = Response.PasswordChanged streamId
            succeed (streamId, ExpectedVersion.Any, [passwordChanged], response)

        | Init
        | Deleted _ -> fail [UserDoesNotExist]
        | UnverifiedUser _ -> fail [UserNotVerified]
        | VerifiedUser _ -> fail [AuthenticationFailed]

    let requestPasswordReset (command: RequestPasswordResetCommand) (_, state) =
        // A user can only request a password reset when it exists and is verified
        match state with
        | VerifiedUser _  ->
            let randomToken = String.random PasswordResetTokenLength
            let token = Token.create randomToken |> Option.get

            let requestedPasswordReset = RequestedPasswordReset {
                RequestedPasswordResetEvent.RequestedAt = DateTime.UtcNow
                ResetToken = PasswordResetToken token
            }

            let streamId = toUserStreamId command.Email
            let response = Response.RequestedPasswordReset streamId
            succeed (streamId, ExpectedVersion.Any, [requestedPasswordReset], response)

        | Init
        | Deleted _ -> fail [UserDoesNotExist]
        | UnverifiedUser _ -> fail [UserNotVerified]

    let verifyPasswordReset (command: VerifyPasswordResetCommand) (_, state) =
        let validatePasswordResetInfo token (resetInfo: PasswordResetInfo) =
            // Check if the token has not expired yet, and is identical
            let resetActiveUntil = resetInfo.RequestedAt.AddMinutes PasswordResetTokenDurationInMinutes
            resetInfo.ResetToken = token && resetActiveUntil > DateTime.UtcNow

        // A user can only reset a password when it exists and matches the token inside the time window
        match state with
        | VerifiedUser (u, Some resetInfo) when validatePasswordResetInfo command.Token resetInfo ->
            let verifiedPasswordReset = VerifiedPasswordReset {
                VerifiedPasswordResetEvent.VerifiedAt = DateTime.UtcNow
                Hash = createHash command.NewPassword
            }

            let streamId = toUserStreamId command.Email
            let claims = buildClaims u.Roles command.Email
            let response = Response.VerifiedPasswordReset (streamId, claims)
            succeed (streamId, ExpectedVersion.Any, [verifiedPasswordReset], response)

        | Init
        | Deleted _ -> fail [UserDoesNotExist]
        | UnverifiedUser _ -> fail [UserNotVerified]
        | VerifiedUser _ -> fail [VerificationFailed]

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
