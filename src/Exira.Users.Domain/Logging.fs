namespace Exira.Users.Domain

module Logging =
    open System

    type ILogger =
        abstract member Information: messageTemplate: string * [<ParamArray>] propertyValues: obj [] -> unit
        abstract member Verbose: messageTemplate: string * [<ParamArray>] propertyValues: obj [] -> unit
        abstract member Fatal: messageTemplate: string * [<ParamArray>] propertyValues: obj [] -> unit
        abstract member Debug: messageTemplate: string * [<ParamArray>] propertyValues: obj [] -> unit
        abstract member Warning: messageTemplate: string * [<ParamArray>] propertyValues: obj [] -> unit

    let mutable internalLogger: ILogger option = None

    let inline private information o messageTemplate propertyValues =
        (^a :(member Information : messageTemplate: string * [<ParamArray>] propertyValues: obj [] -> unit) (o, messageTemplate, propertyValues))

    let inline private verbose o messageTemplate propertyValues =
        (^a :(member Verbose: messageTemplate: string * [<ParamArray>] propertyValues: obj [] -> unit) (o, messageTemplate, propertyValues))

    let inline private fatal o messageTemplate propertyValues =
        (^a :(member Fatal : messageTemplate: string * [<ParamArray>] propertyValues: obj [] -> unit) (o, messageTemplate, propertyValues))

    let inline private debug o messageTemplate propertyValues =
        (^a :(member Debug: messageTemplate: string * [<ParamArray>] propertyValues: obj [] -> unit) (o, messageTemplate, propertyValues))

    let inline private warning o messageTemplate propertyValues =
        (^a :(member Warning: messageTemplate: string * [<ParamArray>] propertyValues: obj [] -> unit) (o, messageTemplate, propertyValues))

    let inline setLogger logger =
        internalLogger <- Some {
            new ILogger with
                member __.Information(messageTemplate, propertyValues) = information logger messageTemplate propertyValues
                member __.Verbose(messageTemplate, propertyValues) = verbose logger messageTemplate propertyValues
                member __.Fatal(messageTemplate, propertyValues) = fatal logger messageTemplate propertyValues
                member __.Debug(messageTemplate, propertyValues) = debug logger messageTemplate propertyValues
                member __.Warning(messageTemplate, propertyValues) = warning logger messageTemplate propertyValues }
