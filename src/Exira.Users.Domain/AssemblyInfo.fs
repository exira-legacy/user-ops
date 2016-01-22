namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Exira.Users.Domain")>]
[<assembly: AssemblyProductAttribute("Exira.Users")>]
[<assembly: AssemblyDescriptionAttribute("Exira.Users is an event sourced microservice to manage users.")>]
[<assembly: AssemblyVersionAttribute("0.5.0")>]
[<assembly: AssemblyFileVersionAttribute("0.5.0")>]
[<assembly: AssemblyMetadataAttribute("githash","81aed07a1cbaa3cc63bc8611cede658d98e471ea")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.5.0"
