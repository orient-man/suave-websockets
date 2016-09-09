#r "packages/Suave/lib/net40/Suave.dll"

printfn "suave Loaded"

open Suave
open Suave.Http
open Suave.Operators
open Suave.Filters
open Suave.Files
open Suave.RequestErrors
open Suave.Sockets.Control
open Suave.WebSocket
open Suave.Web

let echo (webSocket : WebSocket) =
    fun cx ->
        socket {
            let loop = ref true
            while !loop do
                let! msg = webSocket.read()
                match msg with
                | (Text, _, true) ->
                    let data = System.Text.Encoding.UTF8.GetBytes "Hello from server"
                    do! webSocket.send Text (data) true
                | (Ping, _, _) -> do! webSocket.send Pong [||] true
                | (Close, _, _) ->
                    do! webSocket.send Close ([||]) true
                    loop := false
                | _ -> ()
        }

let app : WebPart =
    choose [ path "/websocket" >=> handShake echo
             GET >=> choose [ path "/" >=> file "index.html"
                              browseHome ]
             NOT_FOUND "Found no handlers." ]

startWebServer defaultConfig app
