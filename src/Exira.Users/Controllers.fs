namespace Exira.Users

module Controllers =
    open System.Web.Http
    open GNaP.WebApi.Versioning

    open ActionFilters
    open Model
    open Application

    let logger = Serilogger.logger

    let private await f =
        f |> Async.StartAsTask

    [<RoutePrefix("users")>]
    type UsersController() =
        inherit ApiController()

        [<VersionedRoute>]
        [<JsonInputValidator>]
        member this.Post ([<FromBody>] dto: RegisterDto) =
            logger.Information("Received DTO {@dto}", { dto with Password = "" })
            Dto.Register {
                Email = dto.Email
                Password = dto.Password
            } |> application this |> await

        [<VersionedRoute("changepassword")>]
        [<Authorize>]
        member this.PostChangePassword ([<FromBody>] dto: ChangePasswordDto) =
            logger.Information("Received DTO {@dto}", { dto with PreviousPassword = ""; NewPassword = "" })
            Dto.ChangePassword {
                Email = this.RequestContext.Principal.Identity.Name
                PreviousPassword = dto.PreviousPassword
                NewPassword = dto.NewPassword
            } |> application this |> await

    [<RoutePrefix("users/{user}")>]
    [<JsonInputValidator>]
    type UserController() =
        inherit ApiController()

        [<VersionedRoute("login")>]
        member this.PostLogin ([<FromUri>] user) ([<FromBody>] dto: LoginDto) =
            logger.Information("Received DTO {@dto}", { dto with Password = "" })
            Dto.Login {
                Email = user
                Password = dto.Password
            } |> application this |> await

        [<VersionedRoute("verify")>]
        member this.PostVerify ([<FromUri>] user) ([<FromBody>] dto: VerifyDto) =
            logger.Information("Received DTO {@dto}", dto)
            Dto.Verify {
                Email = user
                Token = dto.Token
            } |> application this |> await

        [<VersionedRoute("requestpasswordreset")>]
        member this.PostRequestPasswordReset ([<FromUri>] user) ([<FromBody>] dto: RequestPasswordResetDto) =
            logger.Information("Received DTO {@dto}", dto)
            Dto.RequestPasswordReset {
                Email = user
            } |> application this |> await

        [<VersionedRoute("verifypasswordreset")>]
        member this.PostVerifyPasswordReset ([<FromUri>] user) ([<FromBody>] dto: VerifyPasswordResetDto) =
            logger.Information("Received DTO {@dto}", { dto with NewPassword = "" })
            Dto.VerifyPasswordReset {
                Email = user
                Token = dto.Token
                NewPassword = dto.NewPassword
            } |> application this |> await