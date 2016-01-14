namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Exira.Users.Domain")>]
[<assembly: AssemblyProductAttribute("Exira.Users")>]
[<assembly: AssemblyDescriptionAttribute("Exira.Users is an event sourced microservice to manage users.")>]
[<assembly: AssemblyVersionAttribute("0.5.45")>]
[<assembly: AssemblyFileVersionAttribute("0.5.45")>]
[<assembly: AssemblyMetadataAttribute("githash","4d606c1419543b9069d1e6ccf28bfa4f1f215108")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.5.45"
