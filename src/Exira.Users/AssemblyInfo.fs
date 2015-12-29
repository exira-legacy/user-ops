namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Exira.Users")>]
[<assembly: AssemblyProductAttribute("Exira.Users")>]
[<assembly: AssemblyDescriptionAttribute("Exira.Users is an event sourced microservice to manage users.")>]
[<assembly: AssemblyVersionAttribute("0.4.0")>]
[<assembly: AssemblyFileVersionAttribute("0.4.0")>]
[<assembly: AssemblyMetadataAttribute("githash","e3d84e2eeabb512bdb1e9ac945dc50172d0a3a6e")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.4.0"
