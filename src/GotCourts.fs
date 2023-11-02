namespace TcFairplay

open System
open System.Collections.Generic
open System.Net.Http
open System.Text.Json

type AuthData = {
    ApiKey: string
    PhpSessionId: string
}

type GotCourtsError = string

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
        let private dateFormat = "yyyy-MM-dd"

        let formatDate (date: DateOnly): string =
            date.ToString(dateFormat)

        let parseDate (s: string): DateOnly =
            DateOnly.ParseExact(s, dateFormat)

        let formatTime (time: TimeOnly): string =
            (time.Hour * 60 + time.Minute) * 60
            |> string

        let calcTime (secondsSinceMidnight: int): TimeOnly =
            TimeOnly(int64 secondsSinceMidnight * 10_000_000L)

    let private toKeyValuePair (x, y) = KeyValuePair(x, y)

    let private clubId = 53223

    let private courtToId = function
        | Court1 -> 8153
        | Court2 -> 8154
        | Court3 -> 8155

    let private idToCourt = function
        | 8153 -> Court1
        | 8154 -> Court2
        | 8155 -> Court3
        | id -> failwithf "Unknown court id '%d." id

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

    let processResponse (rawJson: string): Result<JsonElement, GotCourtsError> =
        let doc = JsonDocument.Parse rawJson
        let success = (doc.RootElement.GetProperty "status").GetBoolean()
        let resp = doc.RootElement.GetProperty "response"

        if success then
            Ok resp
        else
            let errorText = (resp.GetProperty "error").GetString()
            Error errorText

    let createBlocking (client: HttpClient) (blocking: Blocking): Result<Guid list, GotCourtsError> =
        let pairs = buildBlockingPairs blocking
        use content = new FormUrlEncodedContent(pairs |> List.map toKeyValuePair)

        let respMsg = client.PostAsync (blockingUrl, content) |> await
        let rawJson = respMsg.Content.ReadAsStringAsync() |> await

        let getIds (resp: JsonElement) =
            let ids = resp.GetProperty "ids"

            ids.EnumerateArray()
            |> Seq.map (fun id -> Guid.Parse (id.GetString()))
            |> Seq.toList

        processResponse rawJson
        |> Result.map getIds

    let deleteBlocking (client: HttpClient) (id: Guid): Result<unit, string> =
        let url = String.Format("{0}/{1}", blockingUrl, id)
        use reqMsg = new HttpRequestMessage (HttpMethod.Delete, url)
        use respMsg = client.Send(reqMsg)

        respMsg.Content.ReadAsStringAsync() |> await
        |> processResponse
        |> Result.map ignore

    let loadBlockings (client: HttpClient) (date: DateOnly): Result<(Guid * Blocking) list, GotCourtsError> =
        let url = String.Format(listUrlTemplate, clubId, Api.formatDate date)
        let rawJson = client.GetStringAsync(url) |> await

        let getBlockings (resp: JsonElement) =
            let blockings = resp.GetProperty "blockings"

            let parseBlocking (el: JsonElement): (Guid * Blocking) =
                let get (name: string) = el.GetProperty name
                let getString name = (get name).GetString()
                let getInt name = (get name).GetInt32()
                let getTime = getInt >> Api.calcTime

                let guid = Guid.Parse (getString "id")
                let blocking = {
                    Description = (getString "shortDesc")
                    Courts = [getInt "courtId" |> idToCourt]
                    Date = date
                    StartEnd = Some (getTime "startTime", getTime "endTime")
                    Note = getString "note"
                }
                (guid, blocking)

            blockings.EnumerateArray ()
            |> Seq.map parseBlocking
            |> Seq.toList

        processResponse rawJson
        |> Result.map getBlockings
