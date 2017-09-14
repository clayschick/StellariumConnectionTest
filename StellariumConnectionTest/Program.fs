﻿module Program

open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Threading

// System.Net.Sockets.Socket extension methods
type Socket with
    member socket.AsyncAccept() = Async.FromBeginEnd(socket.BeginAccept, socket.EndAccept)
    member socket.AsyncReceive(buffer:byte[], ?offset, ?count) = 
        let offset = defaultArg offset 0
        let count = defaultArg count buffer.Length
        let beginReceive(b,o,c,cb,s) = socket.BeginReceive(b,o,c,SocketFlags.None,cb,s)
        Async.FromBeginEnd(buffer, offset, count, beginReceive, socket.EndReceive)

type Server() =
    static member start() =
        let ipAddr = IPAddress.Parse("192.168.1.71") 
        let endpoint = IPEndPoint(ipAddr, 10001)
        let cts = new CancellationTokenSource()

        let listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        listener.Bind(endpoint)
        listener.Listen(int SocketOptionName.MaxConnections)

        printfn "Server listening on port %d" 10001

        // A recursive call is useful if you want to carry an immutable state
        let rec loop() = async {
            printfn "Waiting for requests"
            let! socket = listener.AsyncAccept()
            printfn "Received connection request"



            return! loop()
        }

        Async.Start(loop(), cancellationToken = cts.Token)
        { new IDisposable with member x.Dispose() = cts.Cancel(); listener.Close() }
 

[<EntryPoint>]
let main argv = 

    let disposable = Server.start()

    printfn "Hit enter to continue"
    Console.Read() |> ignore

    printfn "Disposing of server"

    disposable.Dispose()
    0 // return an integer exit code

