namespace Fazor

module ArgParser =
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

    let sliceRest (l: list<string>) = l.GetSlice(Some 1, Some(l.Length - 1))

    let rec parseCommandLine args optionsSoFar =
        match args with
        // empty list means we're done.
        | [] -> optionsSoFar

        | "--h" :: rest -> 
            Logger.warn "Help argument found, use --h on its own to see the help information!"
            parseCommandLine rest optionsSoFar

        | "--host" :: rest ->
            parseCommandLine
                (sliceRest rest)
                { optionsSoFar with
                      Host = rest.[0] }

        | "--port" :: rest ->
            parseCommandLine
                (sliceRest rest)
                { optionsSoFar with
                      Port = rest.[0] |> int}

        | "--user" :: rest ->
            parseCommandLine
                (sliceRest rest)
                { optionsSoFar with
                      Username = rest.[0] }

        | "--pass" :: rest ->
            parseCommandLine
                (sliceRest rest)
                { optionsSoFar with
                      Password = rest.[0] }

        | "--database" :: rest ->
            parseCommandLine
                (sliceRest rest)
                { optionsSoFar with
                      Database = rest.[0] }

        // handle unrecognized option and keep looping
        | x :: rest ->
            Logger.warn $"Argument {x} is unrecognized"
            parseCommandLine rest optionsSoFar
