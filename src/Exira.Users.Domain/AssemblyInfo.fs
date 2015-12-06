namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Exira.Users.Domain")>]
[<assembly: AssemblyProductAttribute("Exira.Users")>]
[<assembly: AssemblyDescriptionAttribute("Exira.Users is an event sourced microservice to manage users.")>]
[<assembly: AssemblyVersionAttribute("0.3.9")>]
[<assembly: AssemblyFileVersionAttribute("0.3.9")>]
[<assembly: AssemblyMetadataAttribute("githash","08070098a214b3d203f27944c9275d159772adb3")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.3.9"
