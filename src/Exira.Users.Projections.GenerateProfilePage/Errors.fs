namespace Exira.Users.Projections.GenerateProfilePage

[<AutoOpen>]
module internal Errors =
    open Exira.Users.Domain

    type EventStoreError =
    | InvalidEvent
    | ValidateProblem of exn
    | DeserializeProblem of exn
    | StateProblem of Error list