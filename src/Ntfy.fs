namespace TcFairplay

open System.Net
open System.Net.Http

module Ntfy =
    let baseUrl = "https://ntfy.sh"

    let client = new HttpClient()

    let post (topic: string) (content: string) =
        let url = sprintf "%s/%s" baseUrl topic
        use sc = new StringContent(content)

        printfn "Posting text to ntfy.sh."
        let resp = client.PostAsync(url, sc) |> await

        if resp.StatusCode <> HttpStatusCode.OK then
            printfn "Error! Reason: %s" resp.ReasonPhrase
            printfn "Length of topic name: %d chars." topic.Length
        else
            printfn "OK."

        ()
