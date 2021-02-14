namespace Fazor

module Connect =
  open Npgsql.FSharp
  open ArgParser

  let makeConnection _args =
    parseCommandLine _args DefaultOptions
    |> fun args ->
        match (args.Database.Equals ""
               || args.Username.Equals ""
               || args.Password.Equals ""
               || args.Host.Equals "") with
        | false ->
            Logger.info
                $"Connecting to postgresql://{args.Host}:{args.Port}/{args.Database} as '{args.Username}'..."

            Some(
                Sql.host args.Host
                |> Sql.database args.Database
                |> Sql.username args.Username
                |> Sql.password args.Password
                |> Sql.port args.Port
                |> Sql.formatConnectionString
            )
        | true ->
            Logger.error "Missing required argument(s) \'--database\', \'--user\' and \'--pass\'"
            None

  let connectToClient conn =
      match conn with
      | Some conn -> Sql.connect conn
      | None ->
          Logger.error "Could not make a connection to the database, check the error above."
          failwith "Connection failed!"
