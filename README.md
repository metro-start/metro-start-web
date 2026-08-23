# Metro Start Web 1.1

The .NET 10 isolated-worker Azure Functions API used by Metro Start. Production
runs on Azure Flex Consumption in Canada Central.

## Configuration

- `METROSTART_TABLE_CONNECTION_STRING`: Azure Table Storage connection string.
- `WEATHER_API_KEY`: OpenWeatherMap API key.
- `FUNCTIONS_WORKER_RUNTIME`: `dotnet-isolated`.

Secrets belong in local user secrets or Azure application settings and must not
be committed.

## Build and test

```sh
dotnet restore metro-start-web.sln
dotnet test metro-start-web.sln
dotnet publish metro-start-web.csproj --configuration Release --output output
```

To run locally, create an untracked `local.settings.json` containing the three
configuration values above, start Azurite or point at an Azure Storage account,
and run:

```sh
func start
```

Public endpoints are `/api/weather`, `/api/themes`, `/api/newtheme`,
`/api/health`, and `/api/openapi.json`. The theme list is paginated using the
`pageSize` query parameter and `X-Continuation-Token` response header.

## Deploy

Push `master` to run `.github/workflows/master_metrostart-web.yml`. It tests and
publishes the .NET 10 project, signs into Azure using the configured GitHub OIDC
secrets, deploys the `metrostart-web` Function App, and smoke-tests
`https://api.metro-start.com/api/health`.

For a manual deployment after testing:

```sh
func azure functionapp publish metrostart-web --dotnet-isolated
curl --fail https://api.metro-start.com/api/health
```

The deployed Function App must use the .NET 10 isolated runtime and define
`METROSTART_TABLE_CONNECTION_STRING`, `WEATHER_API_KEY`, and
`FUNCTIONS_WORKER_RUNTIME=dotnet-isolated`. Connection-string authentication is
retained intentionally. API release 1.1 remains compatible with Metro Start
10.11 and exposes pagination headers for the browser clients.
