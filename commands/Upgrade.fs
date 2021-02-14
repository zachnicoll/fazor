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
                Logger.info $"Found current version ID {currVer}..."

                Array.filter
                    (fun (elem: string) -> not (elem.Contains(migrationDir () + "initial")))
                    (Directory.GetDirectories(migrationDir ()))
                |> List.ofArray
                |> List.map (fun elem -> elem.Split(migrationDir()).[1])
                |> List.sort
                |> fun l ->
                    if currVer <> "initial" then
                        List.findIndex (fun elem -> elem = currVer) l
                        |> fun i -> snd (List.splitAt (i) l)
                    else
                        l // Use all migrations if only initial migration has been used so far
                |> List.map
                    (fun elem ->
                        Logger.info $"Running upgrade script in {elem}..."
                        extractAndRunScript (elem + "/up.sql") sqlConn
                        updateCurrentVersion elem sqlConn)
                        // Logger.ok $"Successfully ran script!")
                |> ignore

                Logger.complete "Migration upgrade complete!"

    | None -> Logger.error "Upgrade failed!"