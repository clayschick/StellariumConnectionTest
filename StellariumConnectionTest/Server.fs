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

let getHMS(hours : float) =
    let h = Math.Floor(hours)

    let hours_m = (hours - h) * 60.0
    let m = Math.Floor(hours_m)

    let s = (hours_m - m) * 60.0

    // #Evitando los .60..
    //if s >= 59.99 then 
    //    s = 0
    //else
    //    m += 1

    //if m >= 60 then
    //    m = 60-m
    //else
        //h += 1

    (h, m, s)

let getGMS(degrees : float) = 
    // #Evitando operaciones con valores negativos..
    let mutable to_neg : bool = false
    let mutable degs = degrees

    if degs < 0.0 then
        degs = Math.Abs(degs)
    else
        to_neg = true

    let mutable d = Math.Floor(degs)

    let degs_m = (degs - d) * 60.0
    let m = Math.Floor(degs_m)

    let s = (degs_m - m)*60.0

    //#Evitando el .60..
    //if s >= 59.99:
    //    s = 0
    //    m += 1
    //if m >= 60.0:
        //m = 60.0-m
        //d += 1

    if to_neg then
        d = -d
    else
        d = d

    (d, m, s)

let ReadCoordinates(stream : NetworkStream) = async {

    let! response = stream.AsyncRead(20)
    let streamLength = BitConverter.ToInt16(response, 0)

    let raInt = BitConverter.ToUInt32(response, 12)
    let decInt = BitConverter.ToInt32(response, 16)

    let ra_h = float raInt * 12.0 / 2147483648.0
    let dec_h = float decInt * 90.0 / 1073741824.0
    let ra = getHMS(ra_h)
    let dec = getGMS(dec_h)

    if (response.Length > 0) then
        return Some (ra, dec)
    else
        return None
}

let AcceptMessages(stream : NetworkStream) = async {
    while not cts.IsCancellationRequested do
        let! message = Async.Catch(ReadCoordinates(stream))
        match message with
        | Choice1Of2 message ->
            match message with
            | Some ((ra_h,ra_m,ra_s), (dec_d,dec_m,dec_s)) -> printfn "RA = %fh %fm %fs -- DEC = %fd %fm %fs" ra_h ra_m ra_s dec_d dec_m dec_s
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

    let listener = new TcpListener(endpoint)
    listener.Start()

    // start an Async computation on another thread that we can cancel using the disposable below
    // need to figure out how to use the cancellation token in the cts value
    Async.Start <| AcceptClients(listener)

    // return a disposable so that we may handle the lifecycle of the server object ourselves
    // listener has a Dispose method, can I just return the listener and use it's Dispose method?
    // -- TcpLIstener does not have a Dispose method
    { new IDisposable with member x.Dispose() = cts.Cancel(); listener.Stop() }