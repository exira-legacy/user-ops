namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Exira.Users.Domain")>]
[<assembly: AssemblyProductAttribute("Exira.Users")>]
[<assembly: AssemblyDescriptionAttribute("Exira.Users is an event sourced microservice to manage users.")>]
[<assembly: AssemblyVersionAttribute("0.4.11")>]
[<assembly: AssemblyFileVersionAttribute("0.4.11")>]
[<assembly: AssemblyMetadataAttribute("githash","7a6e625863de5afbcc92ce4454bc24b59e9edcbb")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.4.11"
