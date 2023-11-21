namespace TcFairplay

open System

[<AutoOpen>]
module Utils =
    let await t = t |> Async.AwaitTask |> Async.RunSynchronously

    let private chTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Zurich")

    let currentLocalTime () =
        TimeZoneInfo.ConvertTime(DateTime.Now, chTimeZone)

    let formatDateTime (dt: DateTime): string =
        dt.ToString("yyyy-MM-dd HH:mm:ss")

    let formatCurrentTimeStamp (): string =
        formatDateTime (currentLocalTime ())