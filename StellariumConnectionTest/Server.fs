module Server

open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Threading
open vtortola.WebSockets

let cts = new CancellationTokenSource()

type TcpListener with
    member listener.AcceptClientOptionAsync = async {
        let! client = Async.AwaitTask <| listener.AcceptSocketAsync()

        if (not(isNull client)) then
            return Some client
        else
            return None
    }

type Socket with
    member socket.AsyncReadString(buffer:byte[], ?offset, ?count) =
        let offset = defaultArg offset 0
        let count = defaultArg count buffer.Length
        let beginReceive(byte,offset,ccount,cb,s) = socket.BeginReceive(buffer,offset,count,SocketFlags.None,cb,s)
        Async.FromBeginEnd(buffer, offset, count, beginReceive, socket.EndReceive)
    //member socket.AsyncRead = async {

    //    // public abstract Task<WebSocketMessageReadStream> ReadMessageAsync(CancellationToken token);
    //    //let! message = Async.AwaitTask <| x.ReadMessageAsync cancellation.Token
    //    let! message = socket.AsyncReadString


    //    if(not(isNull message)) then
    //        use reader = new StreamReader(message : Stream)
    //        return Some (reader.ReadToEnd())
    //    else
    //        return None
    //}


let AcceptMessages(client : Socket) = async {
    let buffer : byte[] = Array.zeroCreate 1024

    while client.Connected do
        //let! result = Async.Catch client.AsyncRead
        let! result = client.AsyncReadString(buffer)
        printfn "Receiving message : %A" result

        //match result with
        //| Choice1Of2 result -> printfn "Target coordinates : %A" result 
        //| Choice2Of2 error -> printfn "Error reading messages : %A" error

}

let AcceptClients(listener : TcpListener) = async {
    while not cts.IsCancellationRequested do
        let! result = Async.Catch listener.AcceptClientOptionAsync
        match result with
        | Choice1Of2 result ->
            match result with
            | Some client -> Async.Start <| AcceptMessages client
            | None -> ignore()
        | Choice2Of2 error ->
            printfn "Error accepting clients : %A" error
}

let start() =
    let ipAddr = IPAddress.Parse("192.168.1.71") 
    let endpoint = IPEndPoint(ipAddr, 10001)
    let cts = new CancellationTokenSource()

    //let listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
    //listener.Bind(endpoint)
    //listener.Listen(int SocketOptionName.MaxConnections)

    let listener = new TcpListener(endpoint)

    listener.Start()

    // start an Async computation on another thread that we can cancel using the disposable below
    // need to figure out how to use the cancellation token in the cts value
    Async.Start <| AcceptClients listener

    // return a disposable so that we may handle the lifecycle of the server object ourselves
    // listener has a Dispose method, can I just return the listener and use it's Dispose method?
    { new IDisposable with member x.Dispose() = cts.Cancel(); listener.Stop() }