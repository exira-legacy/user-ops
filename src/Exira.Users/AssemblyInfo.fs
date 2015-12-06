namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Exira.Users")>]
[<assembly: AssemblyProductAttribute("Exira.Users")>]
[<assembly: AssemblyDescriptionAttribute("Exira.Users is an event sourced microservice to manage users.")>]
[<assembly: AssemblyVersionAttribute("0.3.7")>]
[<assembly: AssemblyFileVersionAttribute("0.3.7")>]
[<assembly: AssemblyMetadataAttribute("githash","9a2755109bee617ecd08a38748c77755ecc84a08")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.3.7"
