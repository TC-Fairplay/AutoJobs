namespace TcFairplay

open System

type CalendarPlayer =
    | Member of Player
    | Guest

type CalendarEntryContent =
    | Blocking of string
    | ClubReservation of string
    | PlayerReservation of (CalendarPlayer list * bool) // BallMachine

type CalendarEntry = {
    StartEnd: (TimeOnly * TimeOnly)
    Content: CalendarEntryContent
}

type Schedule = {
    Court: CourtNo
    Entries: CalendarEntry list
}

type CalendarDay = {
    Date: DateOnly
    CourtSchedules: Schedule list
}

module Calendar =
    let buildCalendarDay (members: Player list) (dl: DayListing): CalendarDay =
        let findPlayer id = members |> List.tryFind (fun p -> p.Id = id)

        let schedules =
            [Court1; Court2; Court3]
            |> List.map (fun court ->
                let blockings =
                    dl.Blockings
                    |> List.map snd
                    |> List.filter (fun b -> b.Courts |> List.contains court)
                    |> List.map (fun b ->
                        {
                            StartEnd = b.StartEnd |> Option.get
                            Content = Blocking b.Description
                        }
                    )

                let reservations =
                    dl.Reservations
                    |> List.map snd
                    |> List.filter (fun r -> r.Court = court)
                    |> List.map (fun r ->
                        let content =
                            match r.Players with
                            | [] -> ClubReservation r.Text
                            | ps ->
                                let players =
                                    ps
                                    |> List.map (fun p ->
                                        findPlayer p
                                        |> Option.map (fun p -> Member p)
                                        |> Option.defaultValue Guest
                                    )

                                PlayerReservation (players, r.BallMachine)

                        {
                            StartEnd = r.StartEnd
                            Content = content
                        }
                    )

                {
                    Court = court
                    Entries =
                        blockings @ reservations
                        |> List.sortBy (_.StartEnd)
                }
            )

        { Date = dl.Date; CourtSchedules = schedules }
