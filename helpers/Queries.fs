namespace Fazor

module Queries =
    open System
    open System.IO
    open Npgsql.FSharp

    let fetchVersion conn =
        match conn with
        | Some conn ->
            match (Sql.connect conn
                   |> Sql.query "SELECT version FROM fazor_version;"
                   |> Sql.execute (fun read -> read.text "version")) with
            | Ok res -> Some res
            | Error e -> 
              Logger.error $"Error fetching current fazor migration version\n{e}"
              None
        | None -> 
          Logger.error $"Could not make a connection to the database, check the error above."
          None
