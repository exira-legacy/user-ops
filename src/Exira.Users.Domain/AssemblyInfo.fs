namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Exira.Users.Domain")>]
[<assembly: AssemblyProductAttribute("Exira.Users")>]
[<assembly: AssemblyDescriptionAttribute("Exira.Users is an event sourced microservice to manage users.")>]
[<assembly: AssemblyVersionAttribute("0.3.8")>]
[<assembly: AssemblyFileVersionAttribute("0.3.8")>]
[<assembly: AssemblyMetadataAttribute("githash","cb24a74272640f1cf4ede75282b9d90aa25b8b76")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.3.8"
