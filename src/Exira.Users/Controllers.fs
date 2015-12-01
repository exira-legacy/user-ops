namespace Exira.Users

module Controllers =
    open System.Web.Http
    open GNaP.WebApi.Versioning

    open ActionFilters
    open Model
    open Application

    let await f =
        f |> Async.StartAsTask

    [<RoutePrefix("users")>]
    type UsersController() =
        inherit ApiController()

        [<VersionedRoute>]
        [<JsonInputValidator>]
        member this.Post ([<FromBody>] dto: RegisterDto) =
            Dto.Register {
                Email = dto.Email
                Password = dto.Password
            } |> application this |> await

        [<VersionedRoute("changepassword")>]
        [<Authorize>]
        member this.PostChangePassword ([<FromBody>] dto: ChangePasswordDto) =
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
            Dto.Login {
                Email = user
                Password = dto.Password
            } |> application this |> await

        [<VersionedRoute("verify")>]
        member this.PostVerify ([<FromUri>] user) ([<FromBody>] dto: VerifyDto) =
            Dto.Verify {
                Email = user
                Token = dto.Token
            } |> application this |> await

        [<VersionedRoute("requestpasswordreset")>]
        member this.PostRequestPasswordReset ([<FromUri>] user) ([<FromBody>] dto: RequestPasswordResetDto) =
            Dto.RequestPasswordReset {
                Email = user
            } |> application this |> await

        [<VersionedRoute("verifypasswordreset")>]
        member this.PostVerifyPasswordReset ([<FromUri>] user) ([<FromBody>] dto: VerifyPasswordResetDto) =
            Dto.VerifyPasswordReset {
                Email = user
                Token = dto.Token
                NewPassword = dto.NewPassword
            } |> application this |> await