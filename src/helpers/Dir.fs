namespace Fazor

module Dir =
  open System.IO

  let migrationDir () = "./fazor/migrations/"
  let checkMigrationDir () = Directory.Exists(migrationDir ())

  let migrationDirs () = 
    Array.filter
      // Filter out the initial migration
      (fun (elem: string) -> not (elem.Contains("initial")))
      (Directory.GetDirectories(migrationDir ()))
      |> Array.map (fun elem -> elem.Split(migrationDir ()).[1])
