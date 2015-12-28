namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Exira.Users")>]
[<assembly: AssemblyProductAttribute("Exira.Users")>]
[<assembly: AssemblyDescriptionAttribute("Exira.Users is an event sourced microservice to manage users.")>]
[<assembly: AssemblyVersionAttribute("0.4.0")>]
[<assembly: AssemblyFileVersionAttribute("0.4.0")>]
[<assembly: AssemblyMetadataAttribute("githash","08ff4ad1d3a9c2b70b4cff61e02086e54a698811")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.4.0"
