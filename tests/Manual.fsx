#r "bin/Debug/net8.0/AutoJobs.dll"

open System
open TcFairplay

let secrets = Main.getSecretsFromEnvironment ()
let client = GotCourts.createClient secrets.GotCourts

let buildStartEnd startHour endHour =
    (TimeOnly(startHour, 0), TimeOnly(endHour, 0))

let date = DateOnly(2023, 11, 22)
let buildBlocking startEnd = {
    Description = "Test"
    Courts = [Court1]
    Date = date
    StartEnd = startEnd
    Note = "Test."
}

let morning = buildStartEnd 8 12
let blocking = buildBlocking (Some morning)

let res = GotCourts.createBlocking client blocking
