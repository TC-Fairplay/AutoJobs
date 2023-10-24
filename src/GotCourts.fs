namespace TcFairplay

open System
open System.Collections.Generic
open System.Net.Http
open System.Text.Json

type AuthData = {
    ApiKey: string
    PhpSessionId: string
}

module GotCourts =
    let private baseUrl = "https://apps.gotcourts.com"
    let private blockingUrl = baseUrl + "/de/api2/secured/club/blocking"

    // blockings path: /response/blockings/[]
    // path contains object with id, courtId, startTime, endTime, shortDesc, type, note.
    let private listUrlTemplate = baseUrl + "/de/api/secured/club/reservation/list?clubId={0}&date={1}"

    let private apiKeyHeader = "X-GOTCOURTS", "ApiKey=\"{0}\""
    let private cookieHeader = "Cookie", "PHPSESSID={0}"

    let createClient (auth: AuthData): HttpClient =
        let client = new HttpClient()
        let add (name, format) (value: string) =
            client.DefaultRequestHeaders.Add (name, String.Format(format, value))

        add apiKeyHeader auth.ApiKey
        add cookieHeader auth.PhpSessionId
        client

    module private Api =
        let private dateFormat = "{0}-{1}-{2}"

        let formatDate (date: DateOnly): string =
            String.Format(dateFormat, date.Year, date.Month, date.Day)

        let formatTime (time: TimeOnly): string =
            (time.Hour * 60 + time.Minute) * 60
            |> string

    let private toKeyValuePair (x, y) = KeyValuePair(x, y)

    let private clubId = 53223

    let private courtToId = function
        | Court1 -> 8153
        | Court2 -> 8154
        | Court3 -> 8155

    let private buildBlockingPairs (blocking: Blocking): (string * string) list =
        let toCourtPair no = ("courts[]", no |> courtToId |> string)
        let dateTimePairs =
            let timePairs =
                match blocking.StartEnd with
                | Some (s, e) -> [
                        "time[start]", Api.formatTime s
                        "time[end]", Api.formatTime e
                    ]
                | None -> []

            let date = Api.formatDate blocking.Date
            [
                "date", date
                "dateTo", date
                "allDay[disabled]", (blocking.StartEnd |> Option.isSome |> string |> fun s -> s.ToLower())
            ] @ timePairs

        dateTimePairs @
        (blocking.Courts |> List.map toCourtPair) @
        [
            "autoremove", "true"
            "type", "other"
            "description", blocking.Description
            "note", blocking.Note
        ]

    let createBlocking (client: HttpClient) (blocking: Blocking): Guid list =
        let pairs = buildBlockingPairs blocking
        use content = new FormUrlEncodedContent(pairs |> List.map toKeyValuePair)

        let respMsg = client.PostAsync (blockingUrl, content) |> await
        let rawJson = respMsg.Content.ReadAsStringAsync() |> await

        use doc = JsonDocument.Parse rawJson
        let resp = doc.RootElement.GetProperty "response"
        let ids = resp.GetProperty "ids"

        ids.EnumerateArray()
        |> Seq.map (fun id -> Guid.Parse (id.GetString()))
        |> Seq.toList

    let deleteBlocking (client: HttpClient) (id: Guid) =
        let url = String.Format("{0}/{1}", blockingUrl, id)
        use reqMsg = new HttpRequestMessage (HttpMethod.Delete, url)
        use respMsg = client.Send(reqMsg)
        ()
