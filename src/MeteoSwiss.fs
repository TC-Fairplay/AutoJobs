namespace TcFairplay

open System.Net.Http
open System.Text.Json

module MeteoSwiss =
    let private postalCode = "8057"

    // prognosis for 144h, starting today at 0.00.
    let private weatherPrognosisUrl =
        sprintf "https://app-prod-ws.meteoswiss-app.ch/v1/plzDetail?plz=%s00" postalCode

    let getTemperaturePrognosis (): float list =
        use client = new HttpClient()
        let rawJson = client.GetStringAsync weatherPrognosisUrl |> await

        use doc = JsonDocument.Parse rawJson
        let graph = doc.RootElement.GetProperty "graph"
        let temps = graph.GetProperty "temperatureMean1h"

        temps.EnumerateArray()
        |> Seq.map (fun temp -> temp.GetDouble ())
        |> Seq.toList
