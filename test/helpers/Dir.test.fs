namespace Fazor

module DirTest =
    open NUnit.Framework
    open Dir

    [<SetUp>]
    let Setup () =
        ()

    [<Test>]
    let MigrationDirCorrect () =
        Assert.That(migrationDir (), Is.EqualTo("./fazor/migrations/"))
