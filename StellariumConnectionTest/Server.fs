module Server

open System
open System.Net
open System.Net.Sockets
open System.Threading
open System.Reactive
open System.Reactive.Linq
open System.Reactive.Concurrency
open System.Reactive.Disposables
open FSharp.Control.Reactive.Builders
open FSharp.Control.Reactive.Observable
open FSharp.Control.Reactive.Disposables

let cts = new CancellationTokenSource()

type TcpListener with
    member l.AcceptClientAsync = async {
        let! client = Async.AwaitTask <| l.AcceptSocketAsync()
        return client
    }

let getHMS(hours : float) =
    let h = Math.Floor(hours)

    let hours_m = (hours - h) * 60.0
    let m = Math.Floor(hours_m)

    let s = (hours_m - m) * 60.0

    //if s >= 59.99 then 
    //    s = 0
    //else
    //    m += 1

    //if m >= 60 then
    //    m = 60-m
    //else
        //h += 1

    (h, m, s)

let getDMS(degrees : float) = 
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


let readCoordinates(coords : byte[]) =
    let raInt = BitConverter.ToUInt32(coords, 12)
    let decInt = BitConverter.ToInt32(coords, 16)

    let ra_h = float raInt * 12.0 / 2147483648.0
    let dec_h = float decInt * 90.0 / 1073741824.0
    let ra = getHMS(ra_h)
    let dec = getDMS(dec_h)

    if (coords.Length > 0) then
        Some (ra, dec) // <-- Turn this into a record type
    else
        None

let listen() = 
    let ipAddr = IPAddress.Any
    let endpoint = IPEndPoint(ipAddr, 10001)
    let listener = new TcpListener(endpoint)

    listener.Start()

    //observe.While ((fun () -> true), ofAsync(listener.AcceptClientAsync))
    //|> bind( (fun socket -> observe.Yield(new NetworkStream(socket))) )
    //|> bind(fun stream -> observe.While( (fun _ -> true), ofAsync(stream.AsyncRead(20)) ))

    observe.While ((fun () -> true), ofAsync(listener.AcceptClientAsync))
    |> bind( (fun socket -> observe.Yield(new NetworkStream(socket))) )
    |> bind(fun stream -> observe.While( (fun _ -> true), ofAsync(stream.AsyncRead(20)) ))
