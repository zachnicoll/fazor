namespace Fazor

module Upgrade =
  open System.IO
  open Npgsql.FSharp
  open Dir
  open Queries
  open Run

  let runUpgrade (sqlConn: Sql.SqlProps) =
    match fetchVersion sqlConn with
    | Some ver ->
        if ver.Length = 0 then
            Logger.warn $"Could not find fazor_version table, performing initial migration..."
            extractAndRunScript "initial/up.sql" sqlConn // Apply initial migration
        else
            ver.[0]
            |> fun currVer ->
                Logger.info $"Found current version {currVer}..."

                let scriptsToRun = 
                    Array.filter
                        // Filter out the initial migration
                        (fun (elem: string) -> not (elem.Contains("initial")))
                        (Directory.GetDirectories(migrationDir ()))
                    |> Array.map (fun elem -> elem.Split(migrationDir()).[1])
                    |> Array.sort
                    |> fun l ->
                        if currVer <> "initial" && l.Length > 0 then
                            // Split the array at the index of the current version and return the second part
                            // This will be all the scripts that have not been run yet
                            Array.findIndex (fun elem -> elem = currVer) l
                            |> fun i -> snd (Array.splitAt (i) l)
                            |> Array.filter (fun e -> not (e.Contains(currVer)))
                        else
                            l // Use all migrations if only initial migration has been used so far
                
                match scriptsToRun.Length with
                | 0 -> Logger.ok "Database already up to date, no migrations to run."
                | _ ->
                    for script in scriptsToRun do
                        extractAndRunScript (script + "/up.sql") sqlConn
                        updateCurrentVersion script sqlConn

                Logger.complete "Upgrade complete!"

    | None -> Logger.error "Upgrade failed!"
