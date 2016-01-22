open Microsoft.FSharp.Compiler
open Microsoft.FSharp.Compiler.SourceCodeServices
open Microsoft.FSharp.Compiler.SimpleSourceCodeServices

[<EntryPoint>]
let main argv =
    let sss = SimpleSourceCodeServices()

    let arguments =
      [| "D:\\Code\\Temp\\fsharp\\lib\\release\\fsc.exe";
         "-o:obj\\Release\\Exira.Users.dll";
         "--debug:pdbonly";
         "--noframework";
         "--define:TRACE";
         "--optimize+";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\Aether\\lib\\net35\\Aether.dll"; "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\Chiron\\lib\\net40\\Chiron.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\Destructurama.Attributed\\lib\\portable-net45+win+wp80\\Destructurama.Attributed.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\Destructurama.FSharp\\lib\\portable-net45+win+wpa81+wp80+MonoAndroid10+MonoTouch10\\Destructurama.FSharp.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\EventStore.Client\\lib\\net40\\EventStore.ClientAPI.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\Exira.CsrfValidation.Owin\\lib\\net461\\Exira.CsrfValidation.Owin.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\Exira.ErrorHandling\\lib\\net461\\Exira.ErrorHandling.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\Exira.EventStore\\lib\\net461\\Exira.EventStore.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\Exira.EventStore.Owin\\lib\\net461\\Exira.EventStore.Owin.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\src\\Exira.Users.Domain\\bin\\Release\\Exira.Users.Domain.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\Exira.VersionHeader.Owin\\lib\\net46\\Exira.VersionHeader.Owin.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\ExtCore\\lib\\net45\\ExtCore.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\FParsec\\lib\\net40-client\\FParsec.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\FParsec\\lib\\net40-client\\FParsecCS.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\FSharp.Configuration\\lib\\net40\\FSharp.Configuration.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\FSharp.Core\\lib\\net40\\FSharp.Core.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\GNaP.WebApi.Versioning\\lib\\net45\\GNaP.WebApi.Versioning.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\Microsoft.Owin\\lib\\net45\\Microsoft.Owin.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\Microsoft.Owin.Host.SystemWeb\\lib\\net45\\Microsoft.Owin.Host.SystemWeb.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\Microsoft.Owin.Security.Cookies\\lib\\net45\\Microsoft.Owin.Security.Cookies.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\Microsoft.Owin.Security\\lib\\net45\\Microsoft.Owin.Security.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\mscorlib.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\Newtonsoft.Json\\lib\\net45\\Newtonsoft.Json.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\Owin\\lib\\net40\\Owin.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\Serilog\\lib\\net45\\Serilog.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\Serilog\\lib\\net45\\Serilog.FullNetFx.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\Serilog.Sinks.Seq\\lib\\net45\\Serilog.Sinks.Seq.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\FSharp.Configuration\\lib\\net40\\SharpYaml.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\System.Core.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\System.Data.DataSetExtensions.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\System.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\System.IdentityModel.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\System.Net.Http.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\Microsoft.Net.Http\\lib\\net45\\System.Net.Http.Extensions.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\Microsoft.AspNet.WebApi.Client\\lib\\net45\\System.Net.Http.Formatting.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\Microsoft.Net.Http\\lib\\net45\\System.Net.Http.Primitives.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\System.Net.Http.WebRequest.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\System.Numerics.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\System.Web.ApplicationServices.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\Microsoft.AspNet.Cors\\lib\\net45\\System.Web.Cors.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\System.Web.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\System.Web.DynamicData.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\System.Web.Entity.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\System.Web.Extensions.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\Microsoft.AspNet.WebApi.Cors\\lib\\net45\\System.Web.Http.Cors.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\Microsoft.AspNet.WebApi.Core\\lib\\net45\\System.Web.Http.dll";
         "-r:D:\\Code\\GH-Exira\\user-ops\\packages\\Microsoft.AspNet.WebApi.Owin\\lib\\net45\\System.Web.Http.Owin.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\System.Xml.Linq.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Collections.Concurrent.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Collections.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.ComponentModel.Annotations.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.ComponentModel.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.ComponentModel.EventBasedAsync.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Diagnostics.Contracts.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Diagnostics.Debug.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Diagnostics.Tools.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Diagnostics.Tracing.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Dynamic.Runtime.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Globalization.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.IO.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Linq.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Linq.Expressions.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Linq.Parallel.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Linq.Queryable.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Net.NetworkInformation.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Net.Primitives.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Net.Requests.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Net.WebHeaderCollection.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.ObjectModel.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Reflection.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Reflection.Emit.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Reflection.Emit.ILGeneration.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Reflection.Emit.Lightweight.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Reflection.Extensions.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Reflection.Primitives.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Resources.ResourceManager.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Runtime.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Runtime.Extensions.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Runtime.Handles.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Runtime.InteropServices.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Runtime.InteropServices.WindowsRuntime.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Runtime.Numerics.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Runtime.Serialization.Json.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Runtime.Serialization.Primitives.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Runtime.Serialization.Xml.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Security.Principal.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.ServiceModel.Duplex.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.ServiceModel.Http.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.ServiceModel.NetTcp.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.ServiceModel.Primitives.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.ServiceModel.Security.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Text.Encoding.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Text.Encoding.Extensions.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Text.RegularExpressions.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Threading.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Threading.Tasks.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Threading.Tasks.Parallel.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Threading.Timer.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Xml.ReaderWriter.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Xml.XDocument.dll";
         "-r:C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.6.1\\Facades\\System.Xml.XmlSerializer.dll";
         "--target:library";
         "--warn:3";
         "--warnaserror:76";
         "--fullpaths";
         "--flaterrors";
         "--subsystemversion:6.00";
         "--highentropyva+";
         "D:\\Code\\GH-Exira\\user-ops\\src\\Exira.Users\\AssemblyInfo.fs";
         "D:\\Code\\GH-Exira\\user-ops\\src\\Exira.Users\\SerilogRequestContextMiddleware.fs";
         "D:\\Code\\GH-Exira\\user-ops\\src\\Exira.Users\\LogRequestResponseMiddleware.fs";
         "D:\\Code\\GH-Exira\\user-ops\\src\\Exira.Users\\Configuration.fs";
         "D:\\Code\\GH-Exira\\user-ops\\src\\Exira.Users\\Serilogger.fs";
         "D:\\Code\\GH-Exira\\user-ops\\src\\Exira.Users\\OptionConverter.fs";
         "D:\\Code\\GH-Exira\\user-ops\\src\\Exira.Users\\JsonInputValidatorAttribute.fs";
         "D:\\Code\\GH-Exira\\user-ops\\src\\Exira.Users\\Model.fs";
         "D:\\Code\\GH-Exira\\user-ops\\src\\Exira.Users\\Application.fs";
         "D:\\Code\\GH-Exira\\user-ops\\src\\Exira.Users\\Controllers.fs";
         "D:\\Code\\GH-Exira\\user-ops\\src\\Exira.Users\\Startup.fs" |]

    let result = sss.Compile(arguments)

    0
