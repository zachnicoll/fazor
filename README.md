<img src="https://i.imgur.com/RgoFMBa.png" alt="fazor logo" width="75%">
Tired of using dotnet Entity Framework for your database migrations? Try fazor!

## Usage

### Initialise Fazor
```shell
$ fazor init
```

### New Migration
```shell
$ fazor new
```

### Upgrade
```shell
$ fazor upgrade --host localhost --port 5432 --user username --pass password --database my_db
```

### Downgrade
```shell
$ fazor downgrade --host localhost --port 5432 --user username --pass password --database my_db
```

#### Note
`--host` and `--port` will default to `localhost` and `5432` if not supplied

### Get Some Help
```shell
$ fazor help
```
