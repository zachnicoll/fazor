namespace Fazor

module Init =
  open System.IO
  open Templates
  open Dir

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
