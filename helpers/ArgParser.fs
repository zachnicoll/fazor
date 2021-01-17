namespace ArgParser

module ArgParser =
    open Logger

    type CommandLineOptions =
        { Host: string
          Port: int
          Username: string
          Password: string
          Database: string }

    let DefaultOptions: CommandLineOptions =
        { Host = "localhost"
          Port = 5432
          Username = ""
          Password = ""
          Database = "" }

    let rec parseCommandLine args optionsSoFar =
        match args with
        // empty list means we're done.
        | [] -> optionsSoFar

        | "--h" :: remainder -> 
            Logger.warn "Help argument found, use --h on its own to see the help information!"
            parseCommandLine remainder optionsSoFar

        | "--host" :: remainder ->
            parseCommandLine
                (remainder.GetSlice(Some 1, Some(remainder.Length - 1)))
                { optionsSoFar with
                      Host = remainder.[0] }

        | "--port" :: remainder ->
            parseCommandLine
                (remainder.GetSlice(Some 1, Some(remainder.Length - 1)))
                { optionsSoFar with
                      Port = remainder.[0] |> int}

        | "--user" :: remainder ->
            parseCommandLine
                (remainder.GetSlice(Some 1, Some(remainder.Length - 1)))
                { optionsSoFar with
                      Username = remainder.[0] }

        | "--pass" :: remainder ->
            parseCommandLine
                (remainder.GetSlice(Some 1, Some(remainder.Length - 1)))
                { optionsSoFar with
                      Password = remainder.[0] }

        | "--database" :: remainder ->
            parseCommandLine
                (remainder.GetSlice(Some 1, Some(remainder.Length - 1)))
                { optionsSoFar with
                      Database = remainder.[0] }

        // handle unrecognized option and keep looping
        | x :: remainder ->
            Logger.warn $"Argument {x} is unrecognized"
            parseCommandLine remainder optionsSoFar
