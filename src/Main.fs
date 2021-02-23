namespace Fazor

module Main =
    open New
    open Init
    open Connect
    open Upgrade
    open Downgrade

    // TODO: dotnet publish -r linux-x64 -p:PublishSingleFile=true --self-contained false
    // to create standalone binary and publish to a self-hosted site for download.
    // Should only do this once this tool is truly ready to 'go live'

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
