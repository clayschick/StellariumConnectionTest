module Clock

open System
open FSharp.Control.Reactive.Observable

open Scope

let getLST (long : Decimal) (dt : DateTime) =
    let hr = decimal dt.Hour + decimal dt.Minute / 60.0M + decimal dt.Second / 3600.0M

    let dd = decimal dt.Day + hr / 24.0M

    let dy = floor dd

    let yy = 
      if (decimal dt.Month < 3.0M) then (decimal dt.Year - 1.0M) else decimal dt.Year
    
    let mm =
      if (decimal dt.Month < 3.0M) then (decimal dt.Month + 12.0M) else decimal dt.Month

    let gr = 
      if (yy + mm / 100.0M + dy / 10000.0M >= 1582.1015M) then
         2.0M - floor (yy / 100.0M) + floor (floor (yy / 100.0M) / 4.0M)
      else
         0.0M

    let jd = floor (365.25M * yy) + floor (30.6001M * (mm + 1.0M)) + dy + 1720994.5M + gr

    let t : decimal = (jd - 2415020.0M) / 36525.0M 

    let ss = 6.6460656M + 2400.051M * t + 0.00002581M * pown t 2

    let st = (ss / 24.0M - floor (ss / 24.0M)) * 24.0M 

    let sa = st + (dd - floor (dd)) * 24.0M * 1.002737908M

    let sb = sa + long / 15.0M

    let sc =
      if (sb < 0.0M) then
         sb + 24.0M
      else
         sb - 24.0M // <-- This causes a big failure if sb is less than 24.0M and sets sc to negative!

    //let lsth = floor sc // <-- This causes a big failure! It can't be negative
    let lsth = floor sb
    let lstm = floor ((sc - floor(sc)) * 60.0M)
    let lsts = ((sc - floor(sc)) * 60.0M - lstm) * 60.0M

    let millis = int(System.Math.Round(lsts - truncate lsts, 3) * 1000M)

    new System.DateTime(dt.Year, dt.Month, dt.Day, int lsth, int lstm, int (floor lsts), millis)

let getHourAngle (ra : DateTime) (lst : DateTime)  =
    let minutes = lst.Subtract(ra).TotalMinutes;
    let degrees = (decimal minutes * (3.141592653589793M/720M)) * 180M/3.141592653589793M

    degrees

let getTargetTickCount (ticksPerDegree : Decimal) (targetHA : Decimal) =
    let targetTickCount = targetHA * ticksPerDegree
    targetTickCount

let start() =
    let startingCoords = 
        getStartingCoordinates()

    generateTimeSpan startingCoords (fun a -> true) (fun b -> recalculateCurrentCoordinates b) (fun c -> c) (fun _ -> TimeSpan.FromSeconds(1.))

