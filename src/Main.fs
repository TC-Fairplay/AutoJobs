namespace TcFairplay

open System

module Main =
    let private apiKeyName = "GOTCOURTS_API_KEY"
    let private phpSessionIdName = "GOTCOURTS_PHP_SESSION_ID"
    let private ntfyTopicName = "NTFY_TOPIC"

    type Secrets = {
        GotCourts: AuthData
        NtfyTopic: string
    }

    let getSecretsFromEnvironment () =
        let get = Environment.GetEnvironmentVariable
        {
            GotCourts = {
                ApiKey = get apiKeyName
                PhpSessionId = get phpSessionIdName
            }
            NtfyTopic = get ntfyTopicName
        }

    [<EntryPoint>]
    let main (args: string[]): int =
        let secrets = getSecretsFromEnvironment ()

        let result =
            if isNull secrets.GotCourts.ApiKey || isNull secrets.GotCourts.PhpSessionId then
                printfn "💥 Please set environment variables '%s' and '%s'." apiKeyName phpSessionIdName
                1
            else
                let consoleLogger = Logger.createConsoleLogger ()
                let log =
                    if String.IsNullOrEmpty(secrets.NtfyTopic) then
                        consoleLogger
                    else
                        Logger.createMultiLogger [
                            consoleLogger
                            Logger.createStringLogger (Ntfy.post secrets.NtfyTopic)
                        ]

                try
                    try
                        use gotCourtsClient = GotCourts.createClient secrets.GotCourts
                        let res = Jobs.groundFrostCheck log gotCourtsClient
                        if Result.isOk res then 0 else 1

                    with
                    | exn ->
                        log.Write (Error, "💥", sprintf "Exception thrown: %A." exn)
                        1

                finally
                    log.Close ()

        result
