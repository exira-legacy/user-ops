namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Exira.Users.Domain")>]
[<assembly: AssemblyProductAttribute("Exira.Users")>]
[<assembly: AssemblyDescriptionAttribute("Exira.Users is an event sourced microservice to manage users.")>]
[<assembly: AssemblyVersionAttribute("0.5.50")>]
[<assembly: AssemblyFileVersionAttribute("0.5.50")>]
[<assembly: AssemblyMetadataAttribute("githash","14d6fb53d510bd97db58f7f269816d77961fa11a")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.5.50"
