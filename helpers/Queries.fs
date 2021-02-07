namespace Fazor

module Queries =
    open Npgsql.FSharp
    open Templates

    let fetchVersion (sqlConn: Sql.SqlProps) =
        match (sqlConn
               |> Sql.query "SELECT current FROM fazor_version;"
               |> Sql.execute (fun read -> (read.text "current"))) with
        | Ok res -> Some res
        | Error e ->
          if (e.ToString().Contains("relation \"fazor_version\" does not exist")) then
              Some []
          else
              Logger.error $"Error fetching current fazor migration version\n{e}"
              None

    let runScript script sqlConn =
        match (sqlConn |> Sql.query script |> Sql.executeNonQuery) with
        | Ok _ -> Logger.ok "Successfully executed script!"
        | Error e -> Logger.error $"Script execution failed!\n{e}"

    let updateCurrentVersion currVer sqlConn =
        updateVerScript currVer
        |> fun script -> 
            match (sqlConn |> Sql.query script |> Sql.executeNonQuery) with
                | Ok _ -> Logger.ok "Successfully updated current version!"
                | Error e -> Logger.error $"Could not update current version!\n{e}"
