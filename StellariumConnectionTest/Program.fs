open System
open System.Reactive.Concurrency
open FSharp.Control.Reactive.Observable
open FSharp.Control.Reactive.Builders
open Server
open Clock
//open Scope

[<EntryPoint>]
let main argv = 
    let ticksPerDegree = 10000M
    let ra = System.DateTime.Parse("09/08/2017 5:35:17")
    // Dallas longitude = -96.797M

    Console.Write("Enter your longitude: ")
    let long = Console.ReadLine() |> Decimal.Parse
    
    let getTickCountToTarget = 
        getLST long
        >> getHourAngle ra
        >> getTargetTickCount ticksPerDegree
    
    let currentCoordsObs = Clock.start()
    let targetCoordsObs = Server.listen()
    //let hourAngleObs = Clock.calcHourAngle()

    // If we are at this longitude (-96.797M) and we have the RA of our current location (Polaris)
    // calculate the current Hour Angle ever second

    // If we go to a spot the instance we get there our Hour Angle is a value
    // and the moment we stop on that spot our Hour Angle stops and becomes
    // out of date. So we need to recalculate the HA of our target every second
    // against the HA of the scope.

    // Lets start from the beginning:
    // 1. polar aligned the mount and align the finders 
    // 2. get Polaris is crosshairs of finder and in center of scope
    // 3. tell the program to track coordinates (HA) both the target and the scope
    // What is important
    // - current HA or our target (constant change with time)
    // - current HA of the scope (stops if the scope stops goes as fast as we turn the knobs or release the clutch for course movement)
    // When we start off the HA of Polaris and the HA of the scope will be the same

    let nowObs = observe.While ((fun _ -> true), interval (TimeSpan.FromSeconds(1.)))

    nowObs
    |> map (fun now -> getLST long System.DateTime.UtcNow)
    |> subscribe (fun lst -> printfn "Local Sidereal Time %A" lst)
    |> ignore

    targetCoordsObs
    |> subscribe (fun coords -> printfn "Target Coordinates %A" coords)
    |> ignore

    //currentCoordsObs
    //|> subscribe (fun coords -> printfn "Current Coordinates %A" coords)
    //|> ignore

    printfn "Hit enter to continue"
    Console.Read() |> ignore
    printfn "Disposing of server"
    //disposable.Dispose()
    0 // return an integer exit code
