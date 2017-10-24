module Scope

type RA = {
    Hour : int;
    Minute : int;
    Second : float
}

type DEC = {
    Degrees : int;
    Minute : int;
    Second : float
}

type Coordinates = {
    RA : RA;
    DEC : DEC
}

// Start at Polaris
let getStartingCoordinates() = {
        RA={RA.Hour=2;RA.Minute=31;RA.Second=50.60864031}
        DEC={DEC.Degrees=89;DEC.Minute=15;DEC.Second=51.37314722}
    }

// As the Earth spins, an observer can easily track celestial targets by moving the telescope in only one direction (to the west).
// The Hour Angle of the object will increase with time so if I stop I will have to calculate how far to go to catch up.

let recalculateCurrentCoordinates(coords : Coordinates) =
    coords