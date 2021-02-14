namespace Fazor

module Dir =
  open System.IO

  let migrationDir () = "./fazor/migrations/"
  let checkMigrationDir () = Directory.Exists(migrationDir ())
