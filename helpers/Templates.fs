namespace Templates

module Templates =
    type Direction =
        | UPGRADE
        | DOWNGRADE

    let migrationTemplate date name (direction: Direction) =
        $"/* DO NOT ALTER THESE LINES */
-- DATE {date}
-- NAME {name}
-- TYPE {direction}
/*                          */

-- INSERT MIGRATION SCRIPT BELOW"

    let createFazorTable =
      $"CREATE TABLE IF NOT EXISTS fazor_version (
  current TEXT NOT NULL,
  previous TEXT NOT NULL
);

INSERT INTO fazor_version VALUES ('initial', 'initial');"

    let dropFazorTable = "DROP TABLE IF EXIST fazor_version;"

    let initFazorScript =
        $"/* DO NOT ALTER THIS FILE*/
-- NAME initial
-- TYPE UPGRADE

{createFazorTable}
/*                          */"

    let dropFazorScript =
      $"/* DO NOT ALTER THIS FILE*/
-- NAME initial
-- TYPE UPGRADE
{dropFazorTable}
/*                          */"
