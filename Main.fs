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
    Logger.info "Migrations can be run with the following arguments:"
    Logger.info "\tfazor --host localhost --port 5432 --user username --pass password --database my_db"


[<EntryPoint>]
let main argv =
    match (argv |> Array.toList) with
    | [] -> Logger.error "No arguments found! Use --h to get some help."
    | ["--h"] -> helpMessage()
    | _args ->
        let args:ArgParser.CommandLineOptions = ArgParser.parseCommandLine _args ArgParser.DefaultOptions 

        if args.Database.Equals "" || args.Username.Equals "" || args.Password.Equals "" then
            Logger.error "Missing required arguments \'--database\', \'--user\' or \'--pass\'"
            helpMessage()
        else
            Logger.info $"Performing migration on {args.Host}:{args.Port}@{args.Database} as \"{args.Username}\"..."

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
    
    0