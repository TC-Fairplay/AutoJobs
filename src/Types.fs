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
}
