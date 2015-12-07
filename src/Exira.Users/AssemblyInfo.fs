namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Exira.Users")>]
[<assembly: AssemblyProductAttribute("Exira.Users")>]
[<assembly: AssemblyDescriptionAttribute("Exira.Users is an event sourced microservice to manage users.")>]
[<assembly: AssemblyVersionAttribute("0.3.10")>]
[<assembly: AssemblyFileVersionAttribute("0.3.10")>]
[<assembly: AssemblyMetadataAttribute("githash","c6d299fc0673fd0d6e0fb67a10dce37f0deeaed7")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.3.10"
