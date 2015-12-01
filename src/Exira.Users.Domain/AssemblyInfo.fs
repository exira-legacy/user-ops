namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Exira.Users.Domain")>]
[<assembly: AssemblyProductAttribute("Exira.Users")>]
[<assembly: AssemblyDescriptionAttribute("Exira.Users is an event sourced microservice to manage users.")>]
[<assembly: AssemblyVersionAttribute("0.2.4")>]
[<assembly: AssemblyFileVersionAttribute("0.2.4")>]
[<assembly: AssemblyMetadataAttribute("githash","73496d19f3d63eee8416f5df102258aed6326255")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.2.4"
