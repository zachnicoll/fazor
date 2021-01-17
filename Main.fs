open System
open Npgsql.FSharp
open ArgParser
open Logger

let runMigration connStr =
    connStr
    |> Sql.connect
    |> Sql.query "CREATE TABLE IF NOT EXISTS test_this (
        col1 text NOT NULL,
        col2 integer
    );"
    |> Sql.executeNonQuery

let helpMessage () =
    Logger.info """
    fazor upgrade --host localhost --port 5432 --user username --pass password --database my_db

    ARG                 | DESCRIPTION
    --------------------------------------------------------
    init                | setup fazor in current directory
    --------------------------------------------------------
    upgrade             | run up to latest migration
        |_ --host       |   DB host address (optional)
        |_ --port       |   DB host port (optional)
        |_ --user       |   DB connection username
        |_ --pass       |   DB connection password
        |_ --database   |   DB name
    --------------------------------------------------------
    help                | show help
    --------------------------------------------------------
    """


[<EntryPoint>]
let main argv =
    match (argv |> Array.toList) with
    | [] -> Logger.error "No arguments found! Run fazor help to get some help."
    | ["help"] -> helpMessage()
    | ["init"] -> ()
    | "upgrade"::_args ->
        let args:ArgParser.CommandLineOptions = ArgParser.parseCommandLine _args ArgParser.DefaultOptions 

        if args.Database.Equals "" || args.Username.Equals "" || args.Password.Equals "" then
            Logger.error "Missing required argument(s) \'--database\', \'--user\' and \'--pass\'"
            helpMessage()
        else
            Logger.info $"Performing migration on postgresql://{args.Host}:{args.Port}/{args.Database} as \"{args.Username}\"..."

            let connStr =
                Sql.host args.Host
                |> Sql.database args.Database
                |> Sql.username args.Username
                |> Sql.password args.Password
                |> Sql.port args.Port
                |> Sql.formatConnectionString
                
            match runMigration connStr with
            | Ok _ -> Logger.ok $"Migration succeeded!"
            | Error e -> Logger.error $"Migration failed! Here's the stacktrace: \n\n{e}\n"
    | _ -> Logger.error "No recognisable arguments found. Run fazor help to get some help."
    
    0