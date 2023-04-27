open Microsoft.AspNetCore.Components.WebAssembly.Hosting
open Microsoft.Extensions.DependencyInjection
open ComputeService
open SpawnDev.BlazorJS
open SpawnDev.BlazorJS.WebWorkers
open System.Text.Json.Serialization

module Program =

    [<EntryPoint>]
    let Main args =
        let builder = WebAssemblyHostBuilder.CreateDefault(args)
        builder.RootComponents.Add<Main.MyApp>("#main")

        builder.Services
            .AddJSRuntimeJsonOptions(fun o -> o.Converters.Add(JsonFSharpConverter()))
            .AddBlazorJSRuntime()
            .AddWebWorkerService()
            .AddSingleton<IMyComputeService, MyComputeService>()
        |> ignore

        builder.Build().BlazorJSRunAsync() |> ignore
        0
