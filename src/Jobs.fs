namespace TcFairplay

open System
open System.Net.Http

module Jobs =
    let private allCourts = [Court1; Court2; Court3]
    let private morning = TimeOnly(8, 0)
    let private noon = TimeOnly(12, 0)
    let private groundFrostBlockingTitle = "❄️❄️❄️ Bodenfrost (automatische Sperre) ❄️❄️❄️"

    let groundFrostCheck (gotCourtsClient: HttpClient): unit =
        let now = DateTime.Now
        printfn "🎾 Starting job '❆ Ground Frost ❆' (%s)." (formatTimeStamp ())

        // MeteoSwiss temperature prognosis
        printfn "⛅ Fetching weather prognosis from MeteoSwiss."
        let temps = MeteoSwiss.getTemperaturePrognosis ()
        printfn "⛅ done."

        let minTemp =
            // from today 22.00 until tomorrow 8.00
            temps[22..(24 + 8)]
            |> List.min

        if minTemp <= 0.0 then
            printfn "❄️ Danger of ground frost, temperatur will drop below 0° C in the coming night."
            let tomorrow =
                now.AddDays (1.0)
                |> DateOnly.FromDateTime

            let maxTemp =
                // tomorrow from 8.00 until 16.00
                temps[(24 + 8)..(24 + 16)]
                |> List.max

            let startEnd, endTime =
                if maxTemp > 5.0 then
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
