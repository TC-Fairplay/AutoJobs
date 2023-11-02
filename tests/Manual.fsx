#r "bin/Debug/net7.0/AutoJobs.dll"

open System
open TcFairplay

let auth = Main.getAuthDataFromEnvironment ()
let client = GotCourts.createClient auth

let buildStartEnd startHour endHour =
    (TimeOnly(startHour, 0), TimeOnly(endHour, 0))

let date = DateOnly(2023, 11, 2)
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
