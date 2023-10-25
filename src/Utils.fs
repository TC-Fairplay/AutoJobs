namespace TcFairplay

open System

[<AutoOpen>]
module Utils =
    let await t = t |> Async.AwaitTask |> Async.RunSynchronously

    let formatDateTime (dt: DateTime): string =
        dt.ToString("yyyy-MM-dd HH:mm:ss")

    let formatTimeStamp (): string =
        formatDateTime (DateTime.Now)