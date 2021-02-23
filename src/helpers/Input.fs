namespace Fazor

module Input =
    open System

    let userEnterCount max =
        match Console.ReadLine() with
        | "" -> max
        | other ->
            match Int32.TryParse other with
            | (true, number) -> if number <= max then number else -1
            | (false, _) -> -1
