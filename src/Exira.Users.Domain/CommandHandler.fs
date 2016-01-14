namespace Exira.Users.Domain

module CommandHandler =
    open Commands
    open AccountCommandHandler
    open UserCommandHandler

    let handleAccountCommand es command =
        match command with
        | Create accountCommand -> handleCreateAccount accountCommand es

    let handleUserCommand es command =
        match command with
        | Register userCommand -> handleRegister userCommand es
        | Login userCommand -> handleLogin userCommand es
        | Verify userCommand -> handleVerify userCommand es
        | ChangePassword userCommand -> handleChangePassword userCommand es
        | RequestPasswordReset userCommand -> handleRequestPasswordReset userCommand es
        | VerifyPasswordReset userCommand -> handleVerifyPasswordReset userCommand es

    // TODO: How would you plugin a logger here? Pass in an Serilog ILogger and pass it around?
    let handleCommand es command =
        match command with
        | User userCommand -> handleUserCommand es userCommand
        | Account accountCommand -> handleAccountCommand es accountCommand

