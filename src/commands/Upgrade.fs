namespace Fazor

module Upgrade =
    open System
    open Npgsql.FSharp
    open Dir
    open Queries
    open Run
    open Input

    let runUpgrade (sqlConn: Sql.SqlProps) =
        match fetchVersion sqlConn with
        | Some ver ->
            if ver.Length = 0 then
                Logger.warn $"Could not find fazor_version table, performing initial migration..."
                extractAndRunScript "initial/up.sql" sqlConn // Apply initial migration
            else
                ver.[0]
                |> fun currVer ->
                    Logger.ok $"Found current version '{currVer}'..."

                    let scriptsToRun =
                        migrationDirs ()
                        |> Array.sort
                        |> fun l ->
                            if currVer <> "initial" && l.Length > 0 then
                                // Split the array at the index of the current version and return the second part
                                // This will be all the scripts that have not been run yet
                                Array.findIndex (fun elem -> elem = currVer) l
                                |> fun i -> snd (Array.splitAt (i) l)
                                // Filter out current version because we don't to run it
                                |> Array.filter (fun e -> not (e.Contains(currVer)))
                            else
                                l // Use all migrations if only initial migration has been used so far

                    let max = scriptsToRun.Length

                    match max with
                    | 0 -> Logger.ok "Database already up to date, no migrations to run."
                    | _ ->
                        Logger.info $"There are {max} upgrade scripts that can be run."
                        Logger.info $"Enter the number of scripts you would like to run [default {max}]:"

                        let count = userEnterCount max

                        if count <= 0 then
                            Logger.error
                                "Invalid count supplied. Input must be a positive number less than or equal to the number of scripts to run."
                        else
                            Logger.ok $"Running {count} upgrade scripts..."

                            for i = 0 to count - 1 do
                                extractAndRunScript (scriptsToRun.[i] + "/up.sql") sqlConn
                                updateCurrentVersion scriptsToRun.[i] sqlConn

                            Logger.complete "Upgrade complete!"

        | None -> Logger.error "Upgrade failed!"
