namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Exira.Users.Domain")>]
[<assembly: AssemblyProductAttribute("Exira.Users")>]
[<assembly: AssemblyDescriptionAttribute("Exira.Users is an event sourced microservice to manage users.")>]
[<assembly: AssemblyVersionAttribute("0.5.0")>]
[<assembly: AssemblyFileVersionAttribute("0.5.0")>]
[<assembly: AssemblyMetadataAttribute("githash","bfe0baae34fd2828f3d8746efdf73cde6edc001e")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.5.0"
