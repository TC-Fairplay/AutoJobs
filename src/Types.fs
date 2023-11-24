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
type ReservationId = ReservationId of int
type ReservationOwnership = Player | Club

type Reservation = {
    Ownership: ReservationOwnership
    Players: PlayerId list
    Court: CourtNo
    Date: DateOnly
    StartEnd: (TimeOnly * TimeOnly)
    BallMachine: bool
    Text: string // e.g. "Privatunterricht" (Ownershop = Club)
    Note: string
}

type Player = {
    Id: PlayerId
    LastName: string
    FirstName: string
    Email: string option
    Phone: string option
}