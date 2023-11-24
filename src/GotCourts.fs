namespace TcFairplay

open System
open System.Collections.Generic
open System.Net.Http
open System.Text.Json

type DayListing = {
    Date: DateOnly
    Blockings: (Guid * Blocking) list
    Reservations: (ReservationId * Reservation) list
}

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

    let private toKeyValuePair (x, y) = KeyValuePair(x, y)

    let private clubId = 53223

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
        let pairs = Blocking.toKeyValueMap blocking
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

    let loadDayListing (client: HttpClient) (date: DateOnly): Result<DayListing, GotCourtsError> =
        let url = String.Format(listUrlTemplate, clubId, Api.formatDate date)
        let rawJson = client.GetStringAsync(url) |> await

        let parseList (name: string, parser: JsonElement -> 'T) (resp: JsonElement): 'T list =
            let arr = resp.GetProperty name
            arr.EnumerateArray ()
            |> Seq.map parser
            |> Seq.toList

        let parse el = {
            Date = date
            Blockings = parseList ("blockings", Blocking.parse) el
            Reservations = parseList ("reservations", Reservation.parse) el
        }

        processResponse rawJson
        |> Result.map parse
