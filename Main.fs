namespace Fazor

module Main =
    open System
    open System.IO
    open Npgsql.FSharp
    open Templates
    open Queries

    let runMigration connStr =
        connStr
        |> Sql.connect
        |> Sql.query "CREATE TABLE IF NOT EXISTS test_this (
            col1 text NOT NULL,
            col2 integer
        );"
        |> Sql.executeNonQuery

    let initFazor () =
        match (Directory.Exists("./fazor/migrations")) with
        | false -> 
            Logger.info "Creating fazor migrations directory in current folder..."
            Directory.CreateDirectory "./fazor/migrations/"
            |> fun dir ->
                Logger.info "Creating initial migration..."
                Directory.CreateDirectory (dir.FullName + "initial/")
            |> fun dir ->
                File.WriteAllText(dir.FullName + "up.sql", initFazorScript)
                File.WriteAllText(dir.FullName + "down.sql", dropFazorScript)        
            Logger.complete "Setup completed! Please run fazor upgrade to apply migration!"
        | true ->
            Logger.error "Cannot initialise fazor in current directory, fazor directory already exists!"

    let createMigration (name: String) =
        DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        |> fun time -> Directory.CreateDirectory $"./fazor/migrations/{time}_{name.Replace(' ', '_').Trim()}"
        |> fun dir ->
            File.WriteAllText($"{dir.FullName}/up.sql", migrationTemplate dir.Name)
            File.WriteAllText($"{dir.FullName}/down.sql", migrationTemplate dir.Name)

    let newMigration () =
        match Directory.Exists("./fazor/migrations") with
        | true ->
            Logger.info "Enter the name of the new migration:"
            Console.ReadLine()
            |> fun name ->
                Logger.info $"Creating migration \"{name}\"..."
                createMigration name
            Logger.complete "Migration created successfully!"
        | false ->
            Logger.error "Cannot find fazor migrations directory, please run fazor init"

    let runUpgrade conn =
        match fetchVersion conn with
        | Some ver -> Logger.info $"Found current version ID {ver}..."
        | None -> Logger.error "Upgrade failed!"
        // TODO:
            // read latest migration in the fazor_version table
            // read and run next chronological migration from file
            // update previous and current migration fields in the fazor_version table row
            // repeat until latest migration reached

    let runDowngrade conn =
        match fetchVersion conn with
        | Some ver -> Logger.info $"Found current version ID {ver}..."
        | None -> Logger.error "Downgrade failed!"

    let makeConnection _args =
        ArgParser.parseCommandLine _args ArgParser.DefaultOptions
        |> fun args ->  
            // repeat until latest migration reached

            match (args.Database.Equals "" || args.Username.Equals "" || args.Password.Equals "" || args.Host.Equals "") with
            | false -> 
                Logger.info $"Connecting to postgresql://{args.Host}:{args.Port}/{args.Database} as \"{args.Username}\"..."
                Some (
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

    [<EntryPoint>]
    let main argv =
        match (argv |> Array.toList) with
        | [] -> Logger.error "No arguments found! Run fazor help to get some help."
        | ["help"]  -> Logger.helpMessage()
        | ["init"]  -> initFazor()
        | ["new"]   -> newMigration()
        | "upgrade"::args -> makeConnection args |> runUpgrade
        | "downgrade"::args -> makeConnection args |> runDowngrade
        | _ -> Logger.error "No recognisable arguments found. Run fazor help to get some help."
        
        0