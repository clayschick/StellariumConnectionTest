module Server

open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Net.WebSockets
open System.Threading


// Look at
// http://theburningmonk.com/2011/12/f-from-string-to-byte-array-and-back/
// http://yoestuve.es/blog/communications-between-python-and-stellarium-stellarium-telescope-protocol/ <-- ***
// https://github.com/fcsonline/node-telescope-server/blob/master/servers/stellarium.js
// https://codereview.stackexchange.com/questions/15364/simple-stupid-f-async-telnet-client
// http://www.fssnip.net/1E/title/Async-TCP-Server
// https://github.com/vtortola/WebSocketListener/blob/master/samples/FsharpEchoServer/Program.fs
// https://github.com/vtortola/WebSocketListener/wiki/F%23-Echo-Server

let cts = new CancellationTokenSource()

type TcpListener with
    member listener.AcceptClientOptionAsync = async {
        let! client = Async.AwaitTask <| listener.AcceptSocketAsync()

        if (not(isNull client)) then
            return Some client
        else
            return None
    }

let ReadCoordinates(stream : NetworkStream) = async {

    let! response = stream.AsyncRead(20)
    let streamLength = BitConverter.ToInt16(response, 0)

    let raInt = BitConverter.ToUInt32(response, 12)
    let decInt = BitConverter.ToUInt32(response, 16)

    let ra = float raInt * (Math.PI / float 0x80000000)
    let dec = float decInt * (Math.PI / float 0x80000000)
    let cdec = Math.Cos(dec)

    if (response.Length > 0) then
        return Some struct (ra, dec, cdec)
    else
        return None
}

let AcceptMessages(stream : NetworkStream) = async {
    while not cts.IsCancellationRequested do
        let! message = Async.Catch(ReadCoordinates(stream))
        match message with
        | Choice1Of2 message ->
            match message with
            | Some struct (ra, dec, cdec) -> printfn "RA : %f -- Dec : %f" ra dec
            | None -> ignore()
        | Choice2Of2 error -> 
            printfn "Error reading stream : %A" error
}

let AcceptClients(listener : TcpListener) = async {
    while not cts.IsCancellationRequested do
        let! result = Async.Catch(listener.AcceptClientOptionAsync)
        match result with
        | Choice1Of2 result ->
            match result with
            | Some clientSocket -> Async.Start <| AcceptMessages(new NetworkStream(clientSocket))
            | None -> ignore()
        | Choice2Of2 error ->
            printfn "Error accepting clients : %A" error
}

let start() =
    //let ipAddr = IPAddress.Parse("192.168.1.71")
    let ipAddr = IPAddress.Any
    let endpoint = IPEndPoint(ipAddr, 10001)

    let cts = new CancellationTokenSource()

    //let listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
    //listener.Bind(endpoint)
    //listener.Listen(int SocketOptionName.MaxConnections)

    let listener = new TcpListener(endpoint)
    listener.Start()

    // start an Async computation on another thread that we can cancel using the disposable below
    // need to figure out how to use the cancellation token in the cts value
    Async.Start <| AcceptClients(listener)

    // return a disposable so that we may handle the lifecycle of the server object ourselves
    // listener has a Dispose method, can I just return the listener and use it's Dispose method?
    { new IDisposable with member x.Dispose() = cts.Cancel(); listener.Stop() }