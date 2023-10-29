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

    let groundFrostCheck (gotCourtsClient: HttpClient): unit =
        let now = DateTime.Now
        printfn "🎾 Starting job '❆ Ground Frost ❆' (%s)." (formatTimeStamp ())

        // MeteoSwiss temperature prognosis
        printfn "⛅ Fetching weather prognosis from MeteoSwiss for postal code %s." postalCode
        let temps = MeteoSwiss.getTemperaturePrognosis postalCode
        printfn "⛅ done."

        let minTemp =
            temps[fst nightHoursRange..snd nightHoursRange + 24]
            |> List.min

        if minTemp <= minNightTempLimit then
            printfn "❄️ Danger of ground frost, temperatur will drop to %2.1f° C in the coming night." minTemp
            let tomorrow =
                now.AddDays (1.0)
                |> DateOnly.FromDateTime

            let tomorrowTemps = temps |> List.skip 24 |> List.take 24

            let maxTempTomorrow =
                tomorrowTemps[fst dayHoursRange..snd dayHoursRange]
                |> List.max

            let startEnd, endTime =
                if maxTempTomorrow > minDayTempLimit then
                    printfn "☀️ Temperature will raise above 5° C tomorrow."
                    Some (morning, noon), "until noon"
                else
                    printfn "⛄ Temperature will stay below 5° C tomorrow."
                    None, ""

            // GotCourts blocking
            printfn "⛔ Blocking all courts tomorrow %s on GotCourts." endTime
            try
                let blocking = {
                    Description = groundFrostBlockingTitle
                    Courts = allCourts
                    Date = tomorrow
                    StartEnd = startEnd
                    Note = sprintf "Auto-created at %s." (formatTimeStamp ())
                }
                let guids = GotCourts.createBlocking gotCourtsClient blocking

                guids |> List.iter (printfn "  ⛔ Blocking ID: %A")
                printfn "⛔ done."
                ()
            with
                | exn ->
                    printfn "💥 GotCourt blocking failed."
                    printfn "💥 Info: %A" exn

        else
            printfn "✅ All good, minimum temperature in the coming night: %2.1f° C." minTemp

        printfn "🎾 Finished job '❆ Ground Frost ❆' (%s)." (formatTimeStamp ())
        printfn ""
