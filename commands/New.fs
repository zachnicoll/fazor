namespace Fazor

module New =
  open System
  open System.IO
  open Dir
  open Templates

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
