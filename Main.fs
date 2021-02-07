namespace Fazor

module Main =
    open System
    open System.IO
    open Npgsql.FSharp
    open Templates
    open Queries
    open ArgParser

    let migrationDir () = "./fazor/migrations/"

    let checkMigrationDir () = Directory.Exists(migrationDir ())

    let initFazor () =
        match checkMigrationDir () with
        | true -> Logger.error "Cannot initialise fazor in current directory, fazor directory already exists!"
        | false ->
            Logger.info "Creating fazor migrations directory in current folder..."

            Directory.CreateDirectory(migrationDir ())
            |> fun dir ->
                Logger.info "Creating initial migration..."
                Directory.CreateDirectory(dir.FullName + "initial/")
            |> fun dir ->
                File.WriteAllText(dir.FullName + "up.sql", initFazorScript)
                File.WriteAllText(dir.FullName + "down.sql", dropFazorScript)

            Logger.complete "Setup completed! Please run fazor upgrade to apply migration!"

    let createMigration (name: String) =
        DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        |> fun time -> Directory.CreateDirectory $"{migrationDir ()}{time}_{name.Replace(' ', '_').Trim()}"
        |> fun dir ->
            File.WriteAllText($"{dir.FullName}/up.sql", migrationTemplate dir.Name)
            File.WriteAllText($"{dir.FullName}/down.sql", migrationTemplate dir.Name)

    let newMigration () =
        match checkMigrationDir () with
        | true ->
            Logger.info "Enter the name of the new migration:"

            Console.ReadLine()
            |> fun name ->
                Logger.info $"Creating migration '{name}'..."
                createMigration name

            Logger.complete "Migration created successfully!"
        | false -> Logger.error "Cannot find fazor migrations directory, please run fazor init"

    let extractAndRunScript filePath conn =
        migrationDir () + filePath
        |> fun fullPath ->
            match File.Exists(fullPath) with
            | true ->
                Logger.info $"Found migration script '{filePath}...'"

                File.ReadAllText(fullPath)
                |> fun script -> runScript script conn
            | false -> Logger.error $"Could not find script '{filePath}', aborting!"

    let runUpgrade (sqlConn: Sql.SqlProps) =
        match fetchVersion sqlConn with
        | Some ver ->
            if ver.Length = 0 then
                Logger.warn $"Could not find fazor_version table, performing initial migration..."
                extractAndRunScript "initial/up.sql" sqlConn // Apply initial migration
            else
                ver.[0]
                |> fun currVer ->
                    Logger.info $"Found current version ID {currVer}..."

                    Array.filter
                        (fun (elem: string) -> not (elem.Contains(migrationDir () + "initial")))
                        (Directory.GetDirectories(migrationDir ()))
                    |> List.ofArray
                    |> List.map (fun elem -> elem.Split(migrationDir()).[1])
                    |> List.sort
                    |> fun l ->
                        if currVer <> "initial" then
                            List.findIndex (fun elem -> elem = currVer) l
                            |> fun i -> snd (List.splitAt (i) l)
                        else
                            l // Use all migrations if only initial migration has been used so far
                    |> List.map
                        (fun elem ->
                            Logger.info $"Running upgrade script in {elem}..."
                            extractAndRunScript (elem + "/up.sql") sqlConn
                            updateCurrentVersion elem sqlConn)
                            // Logger.ok $"Successfully ran script!")
                    |> ignore

                    Logger.complete "Migration upgrade complete!"

        | None -> Logger.error "Upgrade failed!"

    let runDowngrade (sqlConn: Sql.SqlProps) =
        match fetchVersion sqlConn with
        | Some ver -> Logger.info $"Found current version ID {ver}..."
        | None -> Logger.error "Downgrade failed!"
        // TODO: implement downgrade script execution

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

    [<EntryPoint>]
    let main argv =
        match (argv |> Array.toList) with
        | [] -> Logger.error "No arguments found! Run fazor help to get some help."
        | [ "help" ] -> Logger.helpMessage ()
        | [ "init" ] -> initFazor ()
        | [ "new" ] -> newMigration ()
        | "upgrade" :: args ->
            makeConnection args
            |> connectToClient
            |> runUpgrade
        | "downgrade" :: args ->
            makeConnection args
            |> connectToClient
            |> runDowngrade
        | _ -> Logger.error "No recognisable arguments found. Run fazor help to get some help."

        0
