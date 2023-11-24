namespace TcFairplay

open System
open System.Text.Json

[<AutoOpen>]
module Codec =
    module private Api =
        let private dateFormat = "yyyy-MM-dd"

        let formatDate (date: DateOnly): string =
            date.ToString(dateFormat)

        let parseDate (s: string): DateOnly =
            DateOnly.ParseExact(s, dateFormat)

        let formatTime (time: TimeOnly): string =
            (time.Hour * 60 + time.Minute) * 60
            |> string

        let calcTime (secondsSinceMidnight: int): TimeOnly =
            TimeOnly(int64 secondsSinceMidnight * 10_000_000L)

        let courtToId = function
            | Court1 -> 8153
            | Court2 -> 8154
            | Court3 -> 8155

        let idToCourt = function
            | 8153 -> Court1
            | 8154 -> Court2
            | 8155 -> Court3
            | id -> failwithf "Unknown court id '%d." id

        let stringToReservationOwner = function
            | "club" -> Club
            | "player" -> Player
            | x -> failwithf "Unknown reservation owner type: %s" x

    let tryGetProp (el: JsonElement) (name: string) =
        let (found, e) = el.TryGetProperty(name)
        if found then Some e else None


    module Blocking =
        let parse (el: JsonElement): (Guid * Blocking) =
            let get (name: string) = el.GetProperty name
            let getString name = (get name).GetString()
            let getInt name = (get name).GetInt32()
            let getTime = getInt >> Api.calcTime

            let guid = Guid.Parse (getString "id")
            let blocking = {
                Description = (getString "shortDesc")
                Courts = [getInt "courtId" |> Api.idToCourt]
                Date = getInt "date" |> unixToDateTime |> DateOnly.FromDateTime
                StartEnd = Some (getTime "startTime", getTime "endTime")
                Note = getString "note"
            }
            (guid, blocking)

        let toKeyValueMap (b: Blocking): (string * string) list =
            let toCourtPair no = ("courts[]", no |> Api.courtToId |> string)
            let dateTimePairs =
                let timePairs =
                    match b.StartEnd with
                    | Some (s, e) -> [
                            "time[start]", Api.formatTime s
                            "time[end]", Api.formatTime e
                        ]
                    | None -> []

                let date = Api.formatDate b.Date
                [
                    "date", date
                    "dateTo", date
                    "allDay[disabled]", (b.StartEnd |> Option.isSome |> string |> fun s -> s.ToLower())
                ] @ timePairs

            dateTimePairs @
            (b.Courts |> List.map toCourtPair) @
            [
                "autoremove", "true"
                "type", "other"
                "description", b.Description
                "note", b.Note
            ]

    module Reservation =
        let parse (el: JsonElement): (ReservationId * Reservation) =
            let get (name: string) =
                tryGetProp el name
                |> Option.defaultWith (fun () ->
                    failwithf "Property '%s' not found in '%s'" name (el.ToString())
                )

            let getString (name: string): string =
                tryGetProp el name
                |> Option.map _.GetString()
                |> Option.defaultValue ""

            let getInt name = (get name).GetInt32()
            let getBool name = (get name).GetBoolean()
            let getTime = getInt >> Api.calcTime

            let id = getInt "id" |> ReservationId
            let player =
                tryGetProp el "playerId"
                |> Option.map _.GetInt32()

            let partners =
                let getId (el: JsonElement) = el.GetProperty("id").GetInt32()

                (get "partners").EnumerateArray ()
                |> Seq.map getId
                |> Seq.toList

            let reservation = {
                Ownership = getString "ownership" |> Api.stringToReservationOwner
                Players = (player |> Option.toList) @ partners |> List.map PlayerId
                Court = getInt "courtId" |> Api.idToCourt
                Date = getInt "date" |> unixToDateTime |> DateOnly.FromDateTime
                StartEnd = (getTime "startTime", getTime "endTime")
                BallMachine = getBool "ballMachine"
                Text =
                    match player with
                    | Some _ -> ""
                    | None -> getString "text"

                Note = getString "note"
            }

            (id, reservation)

    //module Player =
