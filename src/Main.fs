namespace Fazor

module Main =
    open New
    open Init
    open Connect
    open Upgrade
    open Downgrade

    [<EntryPoint>]
    let main argv =
        match (argv |> Array.toList) with
        | [] -> Logger.error "No arguments found! Run fazor help to get some help."
        | [ "help" ] -> Logger.helpMessage ()
        | [ "init" ] -> initFazor ()
        | [ "new" ] -> newMigration ()
        | "upgrade" :: args ->
            makeConnection args
            |> connectToClient
            |> runUpgrade
        | "downgrade" :: args ->
            makeConnection args
            |> connectToClient
            |> runDowngrade
        | _ -> Logger.error "No recognisable arguments found. Run fazor help to get some help."

        0
