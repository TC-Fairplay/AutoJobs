namespace TcFairplay

open System

module Main =
    let private apiKeyName = "GOTCOURTS_API_KEY"
    let private phpSessionIdName = "GOTCOURTS_PHP_SESSION_ID"
    let private ntfyTopicName = "NTFY_TOPIC"

    type Secrets = {
        GotCourts: AuthData
        NtfyTopic: string
    }

    let getSecretsFromEnvironment () =
        let get = Environment.GetEnvironmentVariable
        {
            GotCourts = {
                ApiKey = get apiKeyName
                PhpSessionId = get phpSessionIdName
            }
            NtfyTopic = get ntfyTopicName
        }

    [<EntryPoint>]
    let main (args: string[]): int =
        let secrets = getSecretsFromEnvironment ()

        let result =
            if isNull secrets.GotCourts.ApiKey || isNull secrets.GotCourts.PhpSessionId then
                printfn "💥 Please set environment variables '%s' and '%s'." apiKeyName phpSessionIdName
                1
            else
                let consoleLogger = Logger.createConsoleLogger ()
                let log =
                    if String.IsNullOrEmpty(secrets.NtfyTopic) then
                        consoleLogger
                    else
                        Logger.createMultiLogger [
                            consoleLogger
                            Logger.createStringLogger (Ntfy.post secrets.NtfyTopic)
                        ]

                try
                    try
                        use client = GotCourts.createClient secrets.GotCourts
                        (*
                        let res = Jobs.groundFrostCheck log client
                        if Result.isOk res then 0 else 1
                        [1..30] |> List.iter (fun x ->
                            match GotCourts.loadDayListing client (DateOnly(2023, 9, x)) with
                            | Ok dl ->
                                printfn "DayListing: %A" dl
                            | Result.Error err ->
                                printfn "Error: %A" err
                        )
                        *)

                        let members = GotCourts.loadAllPlayers client

                        let date = DateOnly(2023, 10, 22)
                        let dayListing = GotCourts.loadDayListing client GotCourtsData.clubId date

                        match members, dayListing with
                        | Ok ms, Ok dl ->
                            let calDay = Calendar.buildCalendarDay ms dl

                            calDay.Courts
                            |> List.iter (fun cs ->
                                printfn "# %A" cs.Court

                                cs.Entries
                                |> List.iter (fun entry ->
                                    let (s, e) = entry.StartEnd
                                    let startEnd = sprintf "%A - %A" s e
                                    let text =
                                        match entry.Content with
                                        | Blocking s -> s
                                        | ClubReservation s -> s
                                        | PlayerReservation (ps, ballmachine) ->
                                            ps
                                            |> List.map (
                                                function
                                                | Member m -> sprintf "%s %s" m.FirstName m.LastName
                                                | Guest -> "_GAST_"
                                            )
                                            |> String.concat ", "

                                    printfn "%s %s" startEnd text
                                )
                                printfn ""
                            )

                        | _ ->
                            printfn "Error"

                        0

                    with
                    | exn ->
                        log.Write (Error, "💥", sprintf "Exception thrown: %A." exn)
                        1

                finally
                    log.Close ()

        result
