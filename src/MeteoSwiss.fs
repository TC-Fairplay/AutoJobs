namespace TcFairplay

open System
open System.Net.Http
open System.Text.Json

module MeteoSwiss =
    let private weatherPrognosisUrlTemplate =
        "https://app-prod-ws.meteoswiss-app.ch/v1/plzDetail?plz={0}00"

    // Get temperature prognosis for the next 4 days (144h), starting today at 0.00.
    let getTemperaturePrognosis (postalCode: string): float list =
        use client = new HttpClient()
        let url = String.Format(weatherPrognosisUrlTemplate, postalCode)
        let rawJson = client.GetStringAsync url |> await

        use doc = JsonDocument.Parse rawJson
        let graph = doc.RootElement.GetProperty "graph"
        let temps = graph.GetProperty "temperatureMean1h"

        temps.EnumerateArray()
        |> Seq.map (fun temp -> temp.GetDouble ())
        |> Seq.toList
