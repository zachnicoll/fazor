namespace Fazor

module Downgrade =
    open System.IO
    open Npgsql.FSharp
    open Dir
    open Queries
    open Run
    open Input

    let runDowngrade (sqlConn: Sql.SqlProps) =
        match fetchVersion sqlConn with
        | Some ver ->
            if ver.Length = 0 then
                Logger.warn $"Could not find fazor_version table, performing initial migration..."
                extractAndRunScript "initial/up.sql" sqlConn
            else
                ver.[0]
                |> fun currVer ->
                    Logger.ok $"Found current version '{currVer}'..."

                    let mutable scriptsToRun =
                        migrationDirs ()
                        |> Array.sortDescending
                        |> fun l ->
                            if currVer <> "initial" && l.Length > 0 then
                                // Split the array at the index of the current version and return the first part
                                // This will be all the scripts that have been run so far
                                Array.findIndex (fun elem -> elem = currVer) l
                                |> fun i -> snd (Array.splitAt (i) l)
                            else
                                [||] // Use no migrations if version is 'initial'

                    // Append initial migration to the end of the scripts array
                    scriptsToRun <- Array.append scriptsToRun [| "initial" |]
                    
                    let max = scriptsToRun.Length

                    match max with
                    | 1 -> Logger.ok "Database already on initial version, nothing to downgrade."
                    | _ ->
                        let maxNoInitial = max - 1
                        Logger.info $"There are {maxNoInitial} downgrade scripts that can be run."
                        Logger.info $"Enter the number of scripts you would like to run [default {maxNoInitial}]:"

                        let count = userEnterCount maxNoInitial

                        if count <= 0 then
                            Logger.error
                                "Invalid count supplied. Input must be a positive number less than or equal to the number of scripts to run."
                        else
                            Logger.ok $"Running {count} downgrade scripts..."

                            for i = 0 to count - 1 do
                                extractAndRunScript (scriptsToRun.[i] + "/down.sql") sqlConn
                                updateCurrentVersion scriptsToRun.[i + 1] sqlConn

                            Logger.complete "Downgrade complete!"

        | None -> Logger.error "Downgrade failed!"
