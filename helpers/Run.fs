namespace Fazor

module Run =
  open System.IO
  open Npgsql.FSharp
  open Dir

  let runScript script sqlConn =
    match (sqlConn |> Sql.query script |> Sql.executeNonQuery) with
    | Ok _ -> Logger.ok "Successfully executed script!"
    | Error e -> Logger.error $"Script execution failed!\n{e}"

  let extractAndRunScript filePath conn =
    migrationDir () + filePath
    |> fun fullPath ->
        match File.Exists(fullPath) with
        | true ->
            Logger.info $"Found migration script '{filePath}...'"

            File.ReadAllText(fullPath)
            |> fun script -> runScript script conn
        | false -> Logger.error $"Could not find script '{filePath}', aborting!"

