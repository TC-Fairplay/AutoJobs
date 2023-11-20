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

        let log = Logger.createMultiLogger [
            Logger.createConsoleLogger ()
            Logger.createStringLogger (Ntfy.post secrets.NtfyTopic)
        ]

        let result =
            if isNull secrets.GotCourts.ApiKey || isNull secrets.GotCourts.PhpSessionId then
                log.Write (Error, "💥", sprintf "Please set environment variables '%s' and '%s'." apiKeyName phpSessionIdName)
                1
            else
                try
                    use gotCourtsClient = GotCourts.createClient secrets.GotCourts
                    let res = Jobs.groundFrostCheck log gotCourtsClient
                    if Result.isOk res then 0 else 1

                with
                | exn ->
                    log.Write (Error, "💥", sprintf "Exception thrown: %A." exn)
                    1

        log.Close ()

        result
