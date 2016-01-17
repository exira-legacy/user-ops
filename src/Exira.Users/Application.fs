namespace Exira.Users

module Application =
    open System.Net
    open System.Net.Http
    open System.Web.Http
    open System.Security.Claims

    open EventStore.ClientAPI
    open ExtCore.Control
    open Exira.ErrorHandling
    open Exira.EventStore
    open Exira.Users.Domain

    open Exira.CsrfValidation.Owin
    open Model
    open CommandHandler

    let private webConfig = Configuration.webConfig
    let private logger = Serilogger.logger

    type private ResponseMessage =
    | OK
    | Accepted
    | Created
    | NotFound
    | BadRequest
    | Unauthorized
    | InternalServerError

    let private determineCode response =
        match response with
        | OK -> HttpStatusCode.OK
        | Accepted -> HttpStatusCode.Accepted
        | Created -> HttpStatusCode.Created
        | NotFound -> HttpStatusCode.NotFound
        | BadRequest -> HttpStatusCode.BadRequest
        | Unauthorized -> HttpStatusCode.Unauthorized
        | InternalServerError -> HttpStatusCode.InternalServerError

    let private classifyResponse response =
        match response with
        | Empty
            -> Accepted

        | UserRegistered _
        | AccountCreated _
            -> Created

        | UserLoggedIn _
        | UserVerified _
        | PasswordChanged _
        | RequestedPasswordReset _
        | VerifiedPasswordReset _
            -> OK

    let private classifyError error =
        match error with
        | EmailIsRequired
        | EmailMustBeValid _
        | PasswordIsRequired
        | PasswordIsTooShort _
        | PasswordMustBeValid
        | TokenIsRequired
        | TokenMustBeValid _
        | VerificationFailed

        | UserAlreadyExists
        | UserNotVerified
        | UserDoesNotExist
        | AccountAlreadyExists
        | InvalidState _
            -> BadRequest

        | AuthenticationFailed
            -> Unauthorized

        | SaveException _
        | SaveVersionException _
        | InvalidStateTransition _
        | InternalException _
            -> InternalServerError

    let private primaryError errors =
        errors
        |> List.map classifyError
        |> List.sort
        |> List.head

    let private determineErrorCode errors =
        errors
        |> primaryError
        |> determineCode

    let private determineResponseCode response =
        response
        |> classifyResponse
        |> determineCode

    let private formatResponse (controller:'T when 'T :> ApiController) response  =
        let responseCode = response |> determineResponseCode

        match response with
        | Empty
            -> controller.Request.CreateResponse(responseCode, "")

        | PasswordChanged (StreamId streamId)
        | RequestedPasswordReset (StreamId streamId)
        | AccountCreated (StreamId streamId)
            -> controller.Request.CreateResponse(responseCode, streamId)

        | VerifiedPasswordReset (StreamId streamId, claims)
        | UserVerified (StreamId streamId, claims)
        | UserRegistered (StreamId streamId, claims)
        | UserLoggedIn (StreamId streamId, claims) ->
            let csrf = CsrfValidationHelpers.BuildCsrfClaim()
            let claims = claims @ [csrf]
            //let response = controller.Request.CreateResponse(responseCode, claims |> createToken claims)
            let response = controller.Request.CreateResponse(responseCode, streamId)

            let domain =
                if webConfig.Debug.Authentication.UseCookieDomain then Some webConfig.Web.Authentication.CookieDomain
                else None

            CsrfValidationHelpers.WriteCsrfCookie response csrf webConfig.Web.Authentication.CookiePath domain
            controller.Request.GetOwinContext().Authentication.SignIn(new ClaimsIdentity(claims, webConfig.Web.Authentication.AuthenticationName))

            response

    let private matchToResult controller result =
        match result with
        | Success (_, response) ->
            // TODO: Log (StreamId streamId)?
            formatResponse controller response

        | Failure errors ->
            let errorCode = errors |> determineErrorCode
            let error = errors |> formatErrors
            controller.Request.CreateResponse(errorCode, error)

    let private handleException controller result =
        match result with
        | Choice1Of2 x -> x
        | Choice2Of2 ex ->
            logger.Fatal("Internal exception occurred {Exception}", ex.ToString())
            [Error.InternalException ex] |> Failure |> (matchToResult controller)

    let private getConnection (controller: ApiController) =
        let owinEnvironment = controller.Request.GetOwinEnvironment()
        owinEnvironment.["ges.connection"] :?> IEventStoreConnection

    let application controller (dto: Dto) =
        Logging.setLogger logger
        logger.Debug("Received DTO {@DTO}", dto)

        dto
        |> toCommand
        |> bindAsync (controller |> getConnection |> handleCommand)
        |> Async.map (controller |> matchToResult)
        |> Async.Catch
        |> Async.map (controller |> handleException)
