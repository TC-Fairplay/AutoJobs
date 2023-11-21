namespace TcFairplay

open System
open System.Net.Http

module Jobs =
    let private postalCode = "8057"

    let private nightHoursRange = (22, 8)
    let private minNightTempLimit = 0.0

    let private dayHoursRange = (8, 16)
    let private minDayTempLimit = 5.0

    let private allCourts = [Court1; Court2; Court3]
    let private morning = TimeOnly(8, 0)
    let private noon = TimeOnly(12, 0)
    let private groundFrostBlockingTitle = "❄️❄️❄️ Bodenfrost (automatische Sperre) ❄️❄️❄️"

    let private blockCourts (log: Logger) (gotCourtsClient: HttpClient) (blocking: Blocking): Result<unit, GotCourtsError> =
        let blocking = {
            blocking with Note = sprintf "Auto-created at %s." (formatCurrentTimeStamp ())
        }

        let timeWindow =
            let toStr (t: TimeOnly) = t.ToString("HH:mm")
            match blocking.StartEnd with
            | Some (s, e) -> sprintf "from %s until %s" (toStr s) (toStr e)
            | None -> "for the entire day"

        // GotCourts blocking
        log.Write (Warn, "⛔", sprintf "Blocking all courts tomorrow %s on GotCourts." timeWindow)
        log.StartBlock ()

        let result =
            match GotCourts.createBlocking gotCourtsClient blocking with
            | Ok guids ->
                guids |> List.iter (fun guid -> log.Write (Warn, "⛔", sprintf "Blocking ID: %A" guid))
                Ok ()

            | Result.Error text ->
                log.Write (Error, "💥", "GotCourt blocking failed.")
                log.Write (Error, "💥", sprintf "Info: %s" text)
                Result.Error text

        log.EndBlock ()

        result

    let groundFrostCheck (log: Logger) (gotCourtsClient: HttpClient): Result<unit, GotCourtsError> =
        let now = currentLocalTime ()
        log.Write (Info, "🎾", "Starting job '❆ Ground Frost ❆'.")
        log.StartBlock ()

        // MeteoSwiss temperature prognosis
        log.Write (Info, "⛅", sprintf "Fetching weather prognosis from MeteoSwiss for postal code %s." postalCode)
        log.StartBlock()
        let temps = MeteoSwiss.getTemperaturePrognosis postalCode
        log.EndBlock()

        let minTemp =
            temps[fst nightHoursRange..snd nightHoursRange + 24]
            |> List.min

        let result =
            if minTemp <= minNightTempLimit then
                log.Write (Warn, "❄️", sprintf "Danger of ground frost, temperatur will drop to %2.1f° C in the coming night." minTemp)
                log.StartBlock ()
                let tomorrow =
                    now.AddDays (1.0)
                    |> DateOnly.FromDateTime

                // skip today, take tomorrow
                let tomorrowTemps = temps |> List.skip 24 |> List.take 24

                let maxTempTomorrow =
                    tomorrowTemps[fst dayHoursRange..snd dayHoursRange]
                    |> List.max

                let startEnd =
                    if maxTempTomorrow > minDayTempLimit then
                        log.Write (Info, "☀️", "Temperature will raise above 5° C tomorrow.")
                        Some (morning, noon)
                    else
                        log.Write (Warn, "⛄", "Temperature will stay below 5° C tomorrow.")
                        None

                let blocking = {
                    Description = groundFrostBlockingTitle
                    Courts = allCourts
                    Date = tomorrow
                    StartEnd = startEnd
                    Note = ""
                }
                let result = blockCourts log gotCourtsClient blocking

                log.EndBlock ()
                result

            else
                log.Write (Info, "✅", sprintf"All good, minimum temperature in the coming night: %2.1f° C." minTemp)
                Ok ()

        log.EndBlock ()

        result
