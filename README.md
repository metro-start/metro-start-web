# Metro Start Web

The .NET 10 isolated-worker Azure Functions API used by Metro Start. Production
runs on Azure Flex Consumption in Canada Central.

## Configuration

- `METROSTART_TABLE_CONNECTION_STRING`: Azure Table Storage connection string.
- `WEATHER_API_KEY`: OpenWeatherMap API key.
- `FUNCTIONS_WORKER_RUNTIME`: `dotnet-isolated`.

Secrets belong in local user secrets or Azure application settings and must not
be committed.

## Development

```sh
dotnet test metro-start-web.sln
func start
```

Public endpoints are `/api/weather`, `/api/themes`, `/api/newtheme`,
`/api/health`, and `/api/openapi.json`. The theme list is paginated using the
`pageSize` query parameter and `X-Continuation-Token` response header.
