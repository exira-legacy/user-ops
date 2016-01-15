namespace Exira.Users

module internal ActionFilters =
    open System.Net
    open System.Net.Http
    open System.Web.Http.Filters
    open System.Web.Http.ModelBinding
    open System.Collections.Generic

    open Exira.Users.Domain

    type JsonInputValidatorAttribute() =
        inherit ActionFilterAttribute()

        let getMessage (e: ModelError) =
            match e.ErrorMessage, e.Exception with
            | m, _ when not (String.isNullOrEmpty m) -> buildError (None, m) |> Some
            | _, ex when not (String.isNullOrEmpty ex.Message) -> buildError (None, ex.Message) |> Some
            | _ -> None

        let mapErrors (kv: KeyValuePair<string, ModelState>) =
            {
                Field = Some kv.Key
                Message = sprintf "Validation errors detected for %s" kv.Key
                Inner = kv.Value.Errors |> Seq.choose getMessage |> Seq.toList |> Some
            }

        let buildInvalidStateError (modelState: ModelStateDictionary) =
            modelState
            |> Seq.filter (fun s -> s.Value.Errors.Count > 0)
            |> Seq.map mapErrors

        override this.OnActionExecuting context =
            if not context.ModelState.IsValid then
                context.Response <- context.Request.CreateResponse(HttpStatusCode.BadRequest, buildInvalidStateError context.ModelState)
            else
                base.OnActionExecuting(context)