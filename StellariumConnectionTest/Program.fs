module Program

open System
open System.Net
open System.Net.Sockets

let rec asyncPrintResponse (stream : NetworkStream) =
    async {
        let response = stream.ReadByte() |> Char.ConvertFromUtf32 //<-- not sure about this format
        Console.Write(response)
        printf "response : %A" |> ignore
        return! asyncPrintResponse stream
    }


let AsyncAcceptClients(listener : TcpListener) =
  async {
    let controlVar = true
    while controlVar do
        let! result = Async.Catch listener.AcceptTcpClient
        match result with
        | Choice1Of2 result -> 
            match result with
                | None -> ignore()
                | Some client -> Async.Start <| AsyncAcceptMessages client
        | Choice2Of2 error -> 
            Log [|"Error Accepting clients: "; error.Message|]
    Log "Server Stop accepting clients"
  }

let endpoint = new IPEndPoint(IPAddress.Parse("192.168.1.71"),10001)

use listener = new TcpListener(endpoint)

[<EntryPoint>]
let main argv = 
    listener.Start()

    Console.ReadKey true |> ignore
    listener.Stop()

    let controlVar = true

    //while controlVar do


    // This is only client side stuff. I need to listen as a server.
    //let client = new System.Net.Sockets.TcpClient()
    //client.Connect("192.168.1.71", 8090)
    //let stream = client.GetStream()
    //asyncPrintResponse stream |> Async.RunSynchronously

    // The telescope control plug-in uses websockets to talk. It connects to us.
    // Create a websocket listener as an observable
    // https://stackoverflow.com/questions/39112800/observing-incoming-websocket-messages-with-reactive-extensions


    // Passing in a Sceduler for ThreadPool access
    //let obs = intervalOn Scheduler.Default (TimeSpan.FromSeconds(2.))

    //obs 
    //|> subscribe(fun i -> printfn "Doing stuff %i" i) 
    //|> ignore

    Console.WriteLine("\nHit enter to continue...");
    Console.Read();

    0 // return an integer exit code

