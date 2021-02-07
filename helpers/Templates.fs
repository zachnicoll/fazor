namespace Fazor

module Templates =
    let migrationTemplate name =
        $"-- FAZOR MIGRATION SCRIPT\n-- NAME {name}\n-- INSERT MIGRATION SCRIPT BELOW\n"

    let initFazorScript =
        migrationTemplate "inital"
        |> fun str -> str.Replace("-- INSERT MIGRATION SCRIPT BELOW", "-- DO NOT CHANGE THIS FILE")
        |> fun str ->
            str
            + "CREATE TABLE IF NOT EXISTS fazor_version (\n\tcurrent TEXT NOT NULL\n);\nINSERT INTO fazor_version VALUES ('initial');\n"

    let dropFazorScript =
        migrationTemplate "inital"
        |> fun str -> str.Replace("-- INSERT MIGRATION SCRIPT BELOW", "-- DO NOT CHANGE THIS FILE")
        |> fun str -> str + "DROP TABLE IF EXISTS fazor_version;\n"
    
    let updateVerScript currVer =
        $"UPDATE fazor_version SET \"current\" = '{currVer}';"
