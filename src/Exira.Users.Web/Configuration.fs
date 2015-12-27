namespace Exira.Users.Web

module Configuration =
    open FSharp.Configuration
    open System.Web.Hosting

    let configPath = HostingEnvironment.MapPath "~/Web.yaml"

    type WebConfig = YamlConfig<"Web.yaml">
    let webConfig = WebConfig()
    webConfig.LoadAndWatch configPath |> ignore
