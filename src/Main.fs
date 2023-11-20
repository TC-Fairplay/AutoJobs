namespace TcFairplay

open System
open System.Net.Http

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

        let log = Logger.createMultiLogger [
            Logger.createConsoleLogger ()
            Logger.createNtfyLogger ""
        ]

        let result =
            if isNull authData.ApiKey || isNull authData.PhpSessionId then
                log.Write (Error, "💥", sprintf "Please set environment variables '%s' and '%s'." apiKeyName phpSessionIdName)
                1
            else
                try
                    use gotCourtsClient = GotCourts.createClient authData
                    let res = Jobs.groundFrostCheck log gotCourtsClient
                    if Result.isOk res then 0 else 1

                with
                | exn ->
                    log.Write (Error, "💥", sprintf "Exception thrown: %A." exn)
                    1

        log.Close ()

        result
