namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Exira.Users")>]
[<assembly: AssemblyProductAttribute("Exira.Users")>]
[<assembly: AssemblyDescriptionAttribute("Exira.Users is an event sourced microservice to manage users.")>]
[<assembly: AssemblyVersionAttribute("0.4.15")>]
[<assembly: AssemblyFileVersionAttribute("0.4.15")>]
[<assembly: AssemblyMetadataAttribute("githash","dba2952a2ed63e8d505cac01ca57d20f85903951")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.4.15"
