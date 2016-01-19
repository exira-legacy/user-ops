namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Exira.Users")>]
[<assembly: AssemblyProductAttribute("Exira.Users")>]
[<assembly: AssemblyDescriptionAttribute("Exira.Users is an event sourced microservice to manage users.")>]
[<assembly: AssemblyVersionAttribute("0.5.0")>]
[<assembly: AssemblyFileVersionAttribute("0.5.0")>]
[<assembly: AssemblyMetadataAttribute("githash","1213ddf0d99acade6a06764edbbbddf04c4b25cb")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.5.0"
