namespace Exira.Users.Web

module Hubs =
    open Microsoft.AspNet.SignalR
    open FSharp.Interop.Dynamic

    type DummyHub() =
        inherit Hub()

        member this.Send (name: string, msg: string) =
            this.Clients.All?broadcastMessage (name, msg) |> ignore