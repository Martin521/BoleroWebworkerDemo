module Main

open Microsoft.AspNetCore.Components
open SpawnDev.BlazorJS.WebWorkers
open Elmish
open Bolero
open Bolero.Html
open ComputeService

type Computer = ComputeInput -> Async<ComputeOutput>

type ButtonState =
    | On of string
    | Off of string

    member this.IsOn_ =
        match this with
        | On _ -> true
        | Off _ -> false
    member this.Text =
        match this with
        | On s
        | Off s -> s

type Model = {computer: Computer option; buttonStates: Map<int, ButtonState>}

type Message =
    | StoreComputer of Computer
    | StartComputation of ComputeInput
    | ProcessResult of ComputeOutput
    | Error of exn

let init (workerService: WebWorkerService) =
    let createComputerAsync () =
        async {
            let! webWorker = workerService.GetWebWorker() |> Async.AwaitTask
            let computer = webWorker.GetService<IMyComputeService>()
            return computer.ComputeTask >> Async.AwaitTask
        }
    let model = {computer = None; buttonStates = Map [for i in [1..5] -> i, Off "00"]}
    let cmd = Cmd.OfAsync.either createComputerAsync () StoreComputer Error
    model, cmd

let update message model =
    match message with
    | StoreComputer compute -> {model with computer = Some compute}, Cmd.none
    | StartComputation(ComputeInput i as ci) ->
        match model.computer with
        | None -> model, Cmd.none
        | Some computer ->
            {model with buttonStates = model.buttonStates.Add(i, On "00")},
            Cmd.OfAsync.either computer ci ProcessResult Error
    | ProcessResult(ComputeOutput(i, s)) -> {model with buttonStates = model.buttonStates.Add(i, Off $"{s}")}, Cmd.none
    | Error exn ->
        printfn $"{exn.Message}"
        model, Cmd.none

let view model dispatch =
    div {
        h1 {
            attr.``class`` "title is-4"
            if model.computer.IsSome then
                """Click a button to compute a "difficult" random number """
            else
                "No webworkers (yet)"
        }
        p {
            for KeyValue(i, buttonState) in model.buttonStates ->
                button {
                    on.click (fun _ -> dispatch (StartComputation(ComputeInput i)))
                    "class" => if buttonState.IsOn_ then "button m-1 is-loading" else "button m-1"
                    buttonState.Text
                }
        }
    }

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    [<Inject>]
    member val WorkerService = Unchecked.defaultof<WebWorkerService> with get, set

    override this.Program =
        let init = fun _ -> init this.WorkerService
        Program.mkProgram init update view
