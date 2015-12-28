namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Exira.Users.Domain")>]
[<assembly: AssemblyProductAttribute("Exira.Users")>]
[<assembly: AssemblyDescriptionAttribute("Exira.Users is an event sourced microservice to manage users.")>]
[<assembly: AssemblyVersionAttribute("0.4.20")>]
[<assembly: AssemblyFileVersionAttribute("0.4.20")>]
[<assembly: AssemblyMetadataAttribute("githash","1732e8f4561ee6a13cd442dde61e77c692748c21")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.4.20"
