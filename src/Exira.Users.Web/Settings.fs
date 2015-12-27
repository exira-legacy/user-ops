namespace Exira.Users.Web

module internal Settings =
    open FSharp.Configuration
    open System.Web.Hosting

    let configPath = HostingEnvironment.MapPath "~/Web.yaml"

    type Configuration = YamlConfig<"Web.yaml">
    let configuration = Configuration()
    configuration.LoadAndWatch configPath |> ignore
