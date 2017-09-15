module Program

open System

open Server
 

[<EntryPoint>]
let main argv = 

    let disposable = Server.start()

    printfn "Hit enter to continue"
    Console.Read() |> ignore

    printfn "Disposing of server"

    disposable.Dispose()
    0 // return an integer exit code

