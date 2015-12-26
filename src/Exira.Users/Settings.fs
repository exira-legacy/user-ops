namespace Exira.Users

module internal Settings =
    open System.Reflection
    open System.IO
    open FSharp.Configuration

    let private executablePath = Assembly.GetEntryAssembly().Location |> Path.GetDirectoryName
    let private configPath = Path.Combine(executablePath, "Configuration.yaml")

    type Configuration = YamlConfig<"Configuration.yaml">
    let configuration = Configuration()
    configuration.LoadAndWatch configPath |> ignore
