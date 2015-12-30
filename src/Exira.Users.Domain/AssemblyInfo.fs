namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Exira.Users.Domain")>]
[<assembly: AssemblyProductAttribute("Exira.Users")>]
[<assembly: AssemblyDescriptionAttribute("Exira.Users is an event sourced microservice to manage users.")>]
[<assembly: AssemblyVersionAttribute("0.4.0")>]
[<assembly: AssemblyFileVersionAttribute("0.4.0")>]
[<assembly: AssemblyMetadataAttribute("githash","c1acc2b8e9284676b9a050550fb7ead66d9a34e9")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.4.0"
