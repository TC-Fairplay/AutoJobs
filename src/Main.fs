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
            use gotCourtsClient = GotCourts.createClient authData
            Jobs.groundFrostCheck gotCourtsClient

            (*
            if args.Length > 0 then
                let date = DateOnly.ParseExact(args[0], "yyyy-MM-dd")
                GotCourts.loadBlockings gotCourtsClient date |> List.iter (printfn "%A")
            *)

            0
