namespace Fazor

module Downgrade =
  open System.IO
  open Npgsql.FSharp
  open Dir
  open Queries
  open Run

  let runDowngrade (sqlConn: Sql.SqlProps) =
    match fetchVersion sqlConn with
    | Some ver -> Logger.info $"Found current version ID {ver}..."
    | None -> Logger.error "Downgrade failed!"
    // TODO: implement downgrade script execution