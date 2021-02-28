namespace Fazor

module Connect =
    open System
    open Npgsql.FSharp
    open ArgParser

    let makeConnection _args =
        parseCommandLine _args DefaultOptions
        |> checkEnvVars
        |> fun args ->

            match (args.Database = None
                   || args.Username = None
                   || args.Password = None) with
            | false ->
                Logger.info
                    $"Connecting to postgresql://{args.Host}:{args.Port}/{args.Database} as '{args.Username}'..."

                Some(
                    Sql.host args.Host.Value
                    |> Sql.database args.Database.Value
                    |> Sql.username args.Username.Value
                    |> Sql.password args.Password.Value
                    |> Sql.port args.Port.Value
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
