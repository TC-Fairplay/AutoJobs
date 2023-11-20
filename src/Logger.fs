namespace TcFairplay

open System
open System.Text

type Symbol = string
type Message = string

type Level = Info | Warn | Error
type Record = Level * Symbol * Message

type Logger = {
    Write: Record -> unit
    StartBlock: unit -> unit
    EndBlock: unit -> unit
    Close: unit -> unit
}

module Logger =
    // create a stateful logger (for single-threaded use only).
    let private createSimpleLogger (sink: string -> unit, close: unit -> unit): Logger =
        let mutable stack: Record list = []
        let print (sym: Symbol) (msg: Message): Unit =
            sink (sprintf "%s %s%s %s" (formatCurrentTimeStamp ()) (String.replicate stack.Length "   ") sym msg)

        let mutable lastRecord: Record option = None
        {
            Write = fun ((lv, sym, msg) as rc) ->
                lastRecord <- Some rc
                print sym msg

            StartBlock = fun () ->
                stack <- (lastRecord |> Option.get)::stack

            EndBlock = fun () ->
                match stack with
                | (lv, sym, _)::tail ->
                    stack <- tail
                    print sym "done."

                | [] -> failwith "Block was ended before it was started."

            Close = close
        }


    // create a console logger.
    let createConsoleLogger (): Logger =
        createSimpleLogger ((fun s -> Console.WriteLine(s)), ignore)

    // create a logger that collects all logged records and POSTs the text to nfty.sh.
    let createNtfyLogger (topic: string): Logger =
        let sb = StringBuilder()
        let close () =
            let log = sb.ToString()
            Console.Write(log) // FIXME: testing only
            // POST to nfty.sh

        createSimpleLogger ((fun s -> sb.AppendFormat("|Nfty|{0}\n", s) |> ignore), close)

    // create a logger that distributes its inputs to a list of loggers.
    let createMultiLogger (loggers: Logger list): Logger =
        let forAll action = loggers |> List.iter action
        {
            Write = fun rc -> forAll (fun l -> l.Write rc)
            StartBlock = fun () -> forAll (fun l -> l.StartBlock ())
            EndBlock = fun () -> forAll (fun l -> l.EndBlock ())
            Close = fun () -> forAll (fun l -> l.Close ())
        }
