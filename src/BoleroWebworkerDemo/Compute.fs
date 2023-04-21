module ComputeService

open System
open System.Threading.Tasks
open System.Text.Json.Serialization

[<JsonFSharpConverter>]
type ComputeInput = ComputeInput of buttonId: int

[<JsonFSharpConverter>]
type ComputeOutput = ComputeOutput of buttonId: int * randomResult: int

// Interface with Task-returning member, as needed for SpawnDev.BlazorJS
type IMyComputeService =
    abstract ComputeTask: ComputeInput -> Task<ComputeOutput>

type MyComputeService() =
    let rand = Random()

    interface IMyComputeService with
        // Just simulating a long-running computation
        member _.ComputeTask(ComputeInput n) =
            task {
                printfn $"starting {n}"
                let res = Seq.init 1000000 (fun _ -> bigint (rand.Next())) |> Seq.sum |> (fun i -> int (i % 100I))
                printfn $"finished {n}"
                return ComputeOutput(n, res)
            }
