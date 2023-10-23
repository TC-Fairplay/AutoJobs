namespace TcFairplay

open System

[<AutoOpen>]
module Utils =
    let await t = t |> Async.AwaitTask |> Async.RunSynchronously

    let formatDateTime (dt: DateTime): string =
        dt.ToString("yyyy-MM-dd hh:mm:ss")

    let formatTimeStamp (): string =
        formatDateTime (DateTime.Now)