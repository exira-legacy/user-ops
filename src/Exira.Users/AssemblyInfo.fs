namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Exira.Users")>]
[<assembly: AssemblyProductAttribute("Exira.Users")>]
[<assembly: AssemblyDescriptionAttribute("Exira.Users is an event sourced microservice to manage users.")>]
[<assembly: AssemblyVersionAttribute("0.4.0")>]
[<assembly: AssemblyFileVersionAttribute("0.4.0")>]
[<assembly: AssemblyMetadataAttribute("githash","7f7b09c710bf6a2598ca952cc45ba65124d7b87d")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.4.0"
