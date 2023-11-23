namespace TcFairplay

open System

type CourtNo =
    | Court1
    | Court2
    | Court3

type Blocking = {
    Description: string
    Courts: CourtNo list
    Date: DateOnly
    StartEnd: (TimeOnly * TimeOnly) option
    Note: string
}

type PlayerId = PlayerId of int
type ReservationOwner = Player | Club

type Reservation = {
    Owner: ReservationOwner
    Players: PlayerId list
    Court: CourtNo
    Date: DateOnly
    StartEnd: (TimeOnly * TimeOnly)
    BallMachine: bool
    Note: string
}

type Player = {
    Id: PlayerId
    LastName: string
    FirstName: string
    Email: string option
    Phone: string option
}