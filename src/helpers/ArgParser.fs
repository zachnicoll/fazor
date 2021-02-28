namespace Fazor

module ArgParser =
    open System
    type CommandLineOptions =
        { mutable Host: string option
          mutable Port: int option
          mutable Username: string option
          mutable Password: string option
          mutable Database: string option }

    let DefaultOptions: CommandLineOptions =
        { Host = None
          Port = None
          Username = None
          Password = None
          Database = None }
    
    let envDb () = "FAZOR_DB"
    let envUsername () = "FAZOR_USERNAME"
    let envPassword () = "FAZOR_PASSWORD"
    let envHost () = "FAZOR_HOST"
    let envPort () = "FAZOR_PORT"

    let sliceRest (l: list<string>) =
        match l.Length with
        | 0 -> []
        | _ -> l.GetSlice(Some 1, Some(l.Length - 1))

    let rec parseCommandLine args optionsSoFar =
        try
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
                          Host = Some(rest.[0]) }

            | "--port" :: rest ->
                parseCommandLine
                    (sliceRest rest)
                    { optionsSoFar with
                          Port = Some(rest.[0] |> int)}

            | "--user" :: rest ->
                parseCommandLine
                    (sliceRest rest)
                    { optionsSoFar with
                          Username = Some(rest.[0]) }

            | "--pass" :: rest ->
                parseCommandLine
                    (sliceRest rest)
                    { optionsSoFar with
                          Password = Some(rest.[0]) }

            | "--database" :: rest ->
                parseCommandLine
                    (sliceRest rest)
                    { optionsSoFar with
                          Database = Some(rest.[0]) }

            // handle unrecognized option and keep looping
            | x :: rest ->
                Logger.warn $"Argument {x} is unrecognized"
                parseCommandLine rest optionsSoFar
        with
            | :? System.ArgumentException -> 
                Logger.warn "You supplied an option with no value!"
                optionsSoFar

    let nullCheck var =
        if isNull var then
            None
        else
            Some(var)
    
    let nullCheckDefault defaultVal var =
        if isNull var then
            Some(defaultVal)
        else
            Some(var)
    
    let checkEnvVars args =
        match args.Database with
        | None -> 
            args.Database <- Environment.GetEnvironmentVariable(envDb ())
            |> nullCheck
        | _ -> ()

        match args.Username with
        | None -> args.Username <- Environment.GetEnvironmentVariable(envUsername ()) |> nullCheck
        | _ -> ()

        match args.Password with
        | None -> args.Password <- Environment.GetEnvironmentVariable(envPassword ()) |> nullCheck
        | _ -> ()

        match args.Host with
        | None -> args.Host <- Environment.GetEnvironmentVariable(envHost ()) |> nullCheckDefault "localhost"
        | _ -> ()

        match args.Port with
        | None ->
            args.Port <-
                    Environment.GetEnvironmentVariable(envPort ())
                    |> nullCheckDefault "5432"
                    |> fun(port) -> 
                        if (port.IsSome) then 
                            Some(port.Value |> int)
                        else
                            None
        | _ -> ()

        args
