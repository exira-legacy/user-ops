namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Exira.Users")>]
[<assembly: AssemblyProductAttribute("Exira.Users")>]
[<assembly: AssemblyDescriptionAttribute("Exira.Users is an event sourced microservice to manage users.")>]
[<assembly: AssemblyVersionAttribute("0.4.18")>]
[<assembly: AssemblyFileVersionAttribute("0.4.18")>]
[<assembly: AssemblyMetadataAttribute("githash","c7ae56d3a20c5b770487214dbf24936ccf15022a")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.4.18"
