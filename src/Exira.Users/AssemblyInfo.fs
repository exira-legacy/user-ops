namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Exira.Users")>]
[<assembly: AssemblyProductAttribute("Exira.Users")>]
[<assembly: AssemblyDescriptionAttribute("Exira.Users is an event sourced microservice to manage users.")>]
[<assembly: AssemblyVersionAttribute("0.2.5")>]
[<assembly: AssemblyFileVersionAttribute("0.2.5")>]
[<assembly: AssemblyMetadataAttribute("githash","47a9e85c9c06914c76511ff44ac506e3378ddd4a")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.2.5"
