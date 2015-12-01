namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Exira.Users.Domain")>]
[<assembly: AssemblyProductAttribute("Exira.Users")>]
[<assembly: AssemblyDescriptionAttribute("Exira.Users is an event sourced microservice to manage users.")>]
[<assembly: AssemblyVersionAttribute("0.2.0.2")>]
[<assembly: AssemblyFileVersionAttribute("0.2.0.2")>]
[<assembly: AssemblyMetadataAttribute("githash","be5cc34643222c7cdf6bb00ae1411d472b58440c")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.2.0.2"
