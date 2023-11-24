namespace TcFairplay

open System

type SchedulePlayer =
    | Member of Player
    | Guest

type ScheduleEntryContent =
    | Blocking of string
    | ClubReservation of string
    | PlayerReservation of (SchedulePlayer list * bool) // BallMachine

type ScheduleEntry = {
    StartEnd: (TimeOnly * TimeOnly)
    Content: ScheduleEntryContent
}

type Schedule = {
    Court: CourtNo
    Entries: ScheduleEntry list
}

type DaySchedule = {
    Date: DateOnly
    Courts: Schedule list
}

module Calendar =
    let buildCalendarDay (members: Player list) (dl: DayListing): DaySchedule =
        let findPlayer id = members |> List.tryFind (fun p -> p.Id = id)

        let schedules =
            [Court1; Court2; Court3]
            |> List.map (fun court ->
                let courtId = GotCourtsData.courtToId court
                let blockings =
                    dl.Blockings
                    |> List.map snd
                    |> List.filter (fun b -> b.Courts |> List.contains courtId)
                    |> List.map (fun b ->
                        {
                            StartEnd = b.StartEnd |> Option.get
                            Content = Blocking b.Description
                        }
                    )

                let reservations =
                    dl.Reservations
                    |> List.map snd
                    |> List.filter (fun r -> r.Court = courtId)
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

        { Date = dl.Date; Courts = schedules }
