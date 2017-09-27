module Program

open System
open FSharp.Control.Reactive.Observable

open Server

[<EntryPoint>]
let main argv = 

    let coordsObs = Server.listen() |> map(fun bytes -> Server.readCoordinates(bytes))

    coordsObs
    |> subscribe(fun something -> printfn "Coordinates %A" something)
    |> ignore

   
    printfn "Hit enter to continue"
    Console.Read() |> ignore

    printfn "Disposing of server"

    //disposable.Dispose()
    0 // return an integer exit code

