namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Exira.Users.Domain")>]
[<assembly: AssemblyProductAttribute("Exira.Users")>]
[<assembly: AssemblyDescriptionAttribute("Exira.Users is an event sourced microservice to manage users.")>]
[<assembly: AssemblyVersionAttribute("0.2.6")>]
[<assembly: AssemblyFileVersionAttribute("0.2.6")>]
[<assembly: AssemblyMetadataAttribute("githash","021be16d2c124a70fe41b9b71652004837359118")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.2.6"
