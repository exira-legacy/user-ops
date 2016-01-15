namespace Exira.Users.Domain

module Logging =
    open System

    type ILogger =
        abstract member Information: messageTemplate: string * [<ParamArray>] propertyValues: obj [] -> unit
        abstract member Verbose: messageTemplate: string * [<ParamArray>] propertyValues: obj [] -> unit
        abstract member Fatal: messageTemplate: string * [<ParamArray>] propertyValues: obj [] -> unit
        abstract member Debug: messageTemplate: string * [<ParamArray>] propertyValues: obj [] -> unit
        abstract member Warning: messageTemplate: string * [<ParamArray>] propertyValues: obj [] -> unit

    let mutable logger: ILogger = {
        new ILogger with
            member __.Information(_, _) = ()
            member __.Verbose(_, _) = ()
            member __.Fatal(_, _) = ()
            member __.Debug(_, _) = ()
            member __.Warning(_, _) = () }

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

    let inline setLogger l =
        logger <- {
            new ILogger with
                member __.Information(messageTemplate, propertyValues) = information l messageTemplate propertyValues
                member __.Verbose(messageTemplate, propertyValues) = verbose l messageTemplate propertyValues
                member __.Fatal(messageTemplate, propertyValues) = fatal l messageTemplate propertyValues
                member __.Debug(messageTemplate, propertyValues) = debug l messageTemplate propertyValues
                member __.Warning(messageTemplate, propertyValues) = warning l messageTemplate propertyValues }
