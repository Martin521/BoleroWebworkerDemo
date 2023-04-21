open Microsoft.AspNetCore.Components.WebAssembly.Hosting
open Microsoft.Extensions.DependencyInjection
open ComputeService
open SpawnDev.BlazorJS
open SpawnDev.BlazorJS.WebWorkers

module Program =

    [<EntryPoint>]
    let Main args =
        let builder = WebAssemblyHostBuilder.CreateDefault(args)
        builder.RootComponents.Add<Main.MyApp>("#main")

        builder.Services
            .AddBlazorJSRuntime()
            .AddWebWorkerService()
            .AddSingleton<IMyComputeService, MyComputeService>()
        |> ignore

        builder.Build().BlazorJSRunAsync() |> ignore
        0
