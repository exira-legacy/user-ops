namespace Exira.Users.Domain

module CommandHandler =
    let handleCommand es command =
        match command with
        | Register userCommand -> handleRegister userCommand es
        | Login userCommand -> handleLogin userCommand es
        | Verify userCommand -> handleVerify userCommand es
        | ChangePassword userCommand -> handleChangePassword userCommand es
        | RequestPasswordReset userCommand -> handleRequestPasswordReset userCommand es
        | VerifyPasswordReset userCommand -> handleVerifyPasswordReset userCommand es
