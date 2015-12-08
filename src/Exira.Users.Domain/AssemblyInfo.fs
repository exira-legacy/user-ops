namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Exira.Users.Domain")>]
[<assembly: AssemblyProductAttribute("Exira.Users")>]
[<assembly: AssemblyDescriptionAttribute("Exira.Users is an event sourced microservice to manage users.")>]
[<assembly: AssemblyVersionAttribute("0.4.12")>]
[<assembly: AssemblyFileVersionAttribute("0.4.12")>]
[<assembly: AssemblyMetadataAttribute("githash","4d0fe454d314735963d3808631c00f3b3f69dc60")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.4.12"
