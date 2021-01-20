namespace Fazor

module Logger =

    open System

    let log =
        let lockObj = obj()
        fun color pre s ->
            lock lockObj (fun _ ->
                Console.ForegroundColor <- color
                printfn $"{pre} {s}"
                Console.ResetColor())

    let complete = log ConsoleColor.Magenta "DONE:"
    let ok = log ConsoleColor.Green "OK:"
    let info = log ConsoleColor.Cyan "INFO:"
    let warn = log ConsoleColor.Yellow "WARN:"
    let error = log ConsoleColor.Red "ERROR:"
    
    let helpMessage () =
        info """
        fazor upgrade --host localhost --port 5432 --user username --pass password --database my_db

        ARG                 | DESCRIPTION
        --------------------------------------------------------
        init                | setup fazor in current directory
        --------------------------------------------------------
        new                 | create a new migration
        --------------------------------------------------------
        upgrade             | run up to latest migration
            |_ --host       |   DB host address (optional)
            |_ --port       |   DB host port (optional)
            |_ --user       |   DB connection username
            |_ --pass       |   DB connection password
            |_ --database   |   DB name
        --------------------------------------------------------
        help                | show help
        --------------------------------------------------------
        """