namespace TcFairplay

open System

type ClubId = ClubId of int
type CourtId = CourtId of int

type Blocking = {
    Description: string
    Courts: CourtId list
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
    Court: CourtId
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