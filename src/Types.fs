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
    Players: PlayerId list
    Court: CourtNo
    Date: DateOnly
    StartEnd: (TimeOnly * TimeOnly) option
    BallMachine: bool
    Owner: ReservationOwner
}

type Validated<'T> = {
    IsValidated: bool
    Value: 'T
}

type Player = {
    Id: PlayerId
    LastName: string
    FirstName: string
    Email: Validated<string> option
    Phone: Validated<string> option
}