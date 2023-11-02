namespace TcFairplay

open System
open System.Net.Http

module private Testing =
    let blocking = {
        Description = "Automatisch erstellte Test-Sperre."
        Courts = [Court1; Court2; Court3]
        Date = DateOnly(2023, 10, 9)
        StartEnd = Some (TimeOnly(8, 0), TimeOnly(9, 0))
        Note = ""
    }

    let ManualTest (gotCourtsClient: HttpClient) =
        let guids = GotCourts.createBlocking gotCourtsClient blocking
        guids |> List.iter (GotCourts.deleteBlocking gotCourtsClient)

module Main =
    let private apiKeyName = "GOTCOURTS_API_KEY"
    let private phpSessionIdName = "GOTCOURTS_PHP_SESSION_ID"

    let getAuthDataFromEnvironment () =
        let get = Environment.GetEnvironmentVariable
        {
            ApiKey = get apiKeyName
            PhpSessionId = get phpSessionIdName
        }

    [<EntryPoint>]
    let main (args: string[]): int =
        let authData = getAuthDataFromEnvironment ()

        if isNull authData.ApiKey || isNull authData.PhpSessionId then
            printfn "💥 Please set environment variables '%s' and '%s'." apiKeyName phpSessionIdName
            1
        else
            try
                use gotCourtsClient = GotCourts.createClient authData
                let res = Jobs.groundFrostCheck gotCourtsClient
                if Result.isOk res then 0 else 1

            with
            | exn ->
                printfn "💥 Exception thrown: %A." exn
                1
