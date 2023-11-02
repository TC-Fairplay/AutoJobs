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

    let private blockCourts (gotCourtsClient: HttpClient) (blocking: Blocking): Result<unit, GotCourtsError> =
        let blocking = {
            blocking with Note = sprintf "Auto-created at %s." (formatCurrentTimeStamp ())
        }

        let timeWindow =
            let toStr (t: TimeOnly) = t.ToString("HH:mm")
            match blocking.StartEnd with
            | Some (s, e) -> sprintf "from %s until%s" (toStr s) (toStr e)
            | None -> "for the entire day"

        // GotCourts blocking
        printfn "⛔ Blocking all courts tomorrow %s on GotCourts." timeWindow
        match GotCourts.createBlocking gotCourtsClient blocking with
        | Ok guids ->
            guids |> List.iter (printfn "  ⛔ Blocking ID: %A")
            printfn "⛔ done."
            Ok ()

        | Error text ->
            printfn "💥 GotCourt blocking failed."
            printfn "💥 Info: %s" text
            Error text

    let groundFrostCheck (gotCourtsClient: HttpClient): Result<unit, GotCourtsError> =
        let now = DateTime.Now
        printfn "🎾 Starting job '❆ Ground Frost ❆' (%s)." (formatCurrentTimeStamp ())

        // MeteoSwiss temperature prognosis
        printfn "⛅ Fetching weather prognosis from MeteoSwiss for postal code %s." postalCode
        let temps = MeteoSwiss.getTemperaturePrognosis postalCode
        printfn "⛅ done."

        let minTemp =
            temps[fst nightHoursRange..snd nightHoursRange + 24]
            |> List.min

        let result =
            if minTemp <= minNightTempLimit then
                printfn "❄️ Danger of ground frost, temperatur will drop to %2.1f° C in the coming night." minTemp
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
                        printfn "☀️ Temperature will raise above 5° C tomorrow."
                        Some (morning, noon)
                    else
                        printfn "⛄ Temperature will stay below 5° C tomorrow."
                        None

                let blocking = {
                    Description = groundFrostBlockingTitle
                    Courts = allCourts
                    Date = tomorrow
                    StartEnd = startEnd
                    Note = ""
                }
                blockCourts gotCourtsClient blocking

            else
                printfn "✅ All good, minimum temperature in the coming night: %2.1f° C." minTemp
                Ok ()

        printfn "🎾 Finished job '❆ Ground Frost ❆' (%s)." (formatCurrentTimeStamp ())
        printfn ""

        result
