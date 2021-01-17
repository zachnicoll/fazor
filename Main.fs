open System
open System.IO
open Npgsql.FSharp
open ArgParser
open Logger
open Templates.Templates

let runMigration connStr =
    connStr
    |> Sql.connect
    |> Sql.query "CREATE TABLE IF NOT EXISTS test_this (
        col1 text NOT NULL,
        col2 integer
    );"
    |> Sql.executeNonQuery

let createMigration name =
    let dateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
    Directory.CreateDirectory($"./fazor/migrations/{dateTime}_{name}") |> ignore
    File.WriteAllText($"./fazor/migrations/{dateTime}_{name}/up.sql", migrationTemplate dateTime name UPGRADE)
    File.WriteAllText($"./fazor/migrations/{dateTime}_{name}/down.sql", migrationTemplate dateTime name DOWNGRADE)

let initFazor () =
    if not (Directory.Exists("./fazor/migrations")) then  
        Logger.info "Creating fazor migrations directory in current folder..."
        Directory.CreateDirectory("./fazor/migrations") |> ignore

        Logger.info "Creating initial migration..."
        Directory.CreateDirectory($"./fazor/migrations/initial") |> ignore
        File.WriteAllText($"./fazor/migrations/initial/up.sql", initFazorScript)
        File.WriteAllText($"./fazor/migrations/initial/down.sql", dropFazorScript)

        Logger.complete "Setup completed! Please run fazor upgrade to apply migration!"
    else
        Logger.error "Cannot initialise fazor in current directory, fazor directory already exists!"

let newMigration () =
    if Directory.Exists("./fazor/migrations") then
        Logger.info "Enter the name of the new migration:"
        let name = Console.ReadLine()
        Logger.info $"Creating migration \"{name}\"..."

        createMigration name

        Logger.complete "Migration created successfully!"
    else
        Logger.error "Cannot find fazor migrations directory, please run fazor init."

let upgradeMigration _args =
    let args:ArgParser.CommandLineOptions = ArgParser.parseCommandLine _args ArgParser.DefaultOptions 

    if args.Database.Equals "" || args.Username.Equals "" || args.Password.Equals "" then
        Logger.error "Missing required argument(s) \'--database\', \'--user\' and \'--pass\'"
        Logger.helpMessage()
    else
        Logger.info $"Performing migration on postgresql://{args.Host}:{args.Port}/{args.Database} as \"{args.Username}\"..."

        let connStr =
            Sql.host args.Host
            |> Sql.database args.Database
            |> Sql.username args.Username
            |> Sql.password args.Password
            |> Sql.port args.Port
            |> Sql.formatConnectionString
        
        // TODO:
            // read latest migration in the fazor_version table

            // read and run next chronological migration from file

            // update previous and current migration fields in the fazor_version table row

            // repeat until latest migration reached


        match runMigration connStr with
        | Ok _ -> Logger.ok $"Migration succeeded!"
        | Error e -> Logger.error $"Migration failed! Here's the stacktrace: \n\n{e}\n"

[<EntryPoint>]
let main argv =
    match (argv |> Array.toList) with
    | [] -> Logger.error "No arguments found! Run fazor help to get some help."
    | ["help"]  -> Logger.helpMessage()
    | ["init"]  -> initFazor()
    | ["new"]   -> newMigration()
    | "upgrade"::_args -> upgradeMigration _args
    | _ -> Logger.error "No recognisable arguments found. Run fazor help to get some help."
    
    0