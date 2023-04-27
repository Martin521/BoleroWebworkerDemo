open System.IO

let project = "src/BoleroWebworkerDemo"
let s3bucket = "blazortest"
let sdk = "net7.0"

let runCommand command captureStandardOutput =
  let winProc = new System.Diagnostics.Process()
  winProc.StartInfo.FileName <- "cmd.exe"
  winProc.StartInfo.Arguments <- "/C " + command
  winProc.StartInfo.RedirectStandardOutput <- captureStandardOutput
  if not <| winProc.Start() then printfn "problem executing command"; false, "" else
  winProc.WaitForExit()
  let output = if captureStandardOutput then winProc.StandardOutput.ReadToEnd() else ""
  winProc.ExitCode = 0, output

let runCmd command = runCommand command false |> fst

let getDirtyDirectories() =
  let getDirs dir = Directory.GetDirectories dir |> Array.toList
  let projectDirs = getDirs "src"
  let combineWith dirs path = dirs |> List.map (fun dir -> Path.Combine(path, dir))
  projectDirs |> List.collect (combineWith ["bin"; "obj"]) |> List.filter Directory.Exists

let rec deleteFolder dir =
  let files = Directory.GetFiles dir |> Array.toList
  let dirs = Directory.GetDirectories dir |> Array.toList
  files |> List.iter File.Delete
  dirs |> List.iter (fun d -> deleteFolder d; Directory.Delete d)

let clean () =
  let dirs = getDirtyDirectories()
  dirs |> List.iter (fun d -> deleteFolder d; Directory.Delete d)
  true

let build () =  // build debug version for Intellisense
  runCmd $"dotnet build {project}"

let local () =
  runCmd $"dotnet run --project {project}"

let publish () =
  runCmd $"dotnet publish -c Release {project}"

let srcFolder = $"{project}/bin/Release/{sdk}/publish/wwwroot/"
let destFolder = $"s3://{s3bucket}/"

let cacheOption = "" // "--cache-control max-age=60"
let excludes = """--exclude "*.gz" --exclude "*.br" """   // Cloudfront provides compression on demand

let copyFolder () = runCmd $"aws s3 cp {srcFolder} {destFolder} {excludes} {cacheOption} --recursive"

let deploy () = publish () && copyFolder ()

let cleanS3() = runCmd $"aws s3 rm --recursive {destFolder}"

let run f =
  let start = System.DateTime.Now
  let res = f ()
  if res then printfn $"ran %.1f{(System.DateTime.Now - start).TotalSeconds} sec"
  else printfn "run ERROR"
  res

match Array.tail fsi.CommandLineArgs with
| [|"clean"|] -> run clean
| [|"build"|] -> run build
| [|"local"|] -> run local
| [|"cleanS3"|] -> run cleanS3
| [|"deploy"|] -> run deploy
| a -> printfn $"unknown command"; false
