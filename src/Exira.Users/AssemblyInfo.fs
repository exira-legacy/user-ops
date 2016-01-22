namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Exira.Users")>]
[<assembly: AssemblyProductAttribute("Exira.Users")>]
[<assembly: AssemblyDescriptionAttribute("Exira.Users is an event sourced microservice to manage users.")>]
[<assembly: AssemblyVersionAttribute("0.5.0")>]
[<assembly: AssemblyFileVersionAttribute("0.5.0")>]
[<assembly: AssemblyMetadataAttribute("githash","98789c22fb9dbcec6f79028b6f039e8bccb4aa9c")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.5.0"
