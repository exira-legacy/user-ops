module Exira.Users.Domain.Tests.AccountProperties

open FsCheck.Xunit
open Exira.Users.Domain.Commands

[<Property>]
let ``when creating an account`` (cmd: CreateAccountCommand) =
    printfn "%A" cmd
    true