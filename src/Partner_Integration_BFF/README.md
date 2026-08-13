# Partner Integration BFF

ASP.NET Core 8 API for accepting partner transactions, verifying the partner, and publishing accepted transactions to RabbitMQ.

## Local architecture

When running in the `Development` environment:

1. `POST /api/v1/partner/transactions` validates the request.
2. The API calls its local mock partner endpoint at `/api/mock/partners/{partnerId}`.
3. A verified transaction is published to the durable RabbitMQ queue `partner-transactions`.

The mock partner endpoint intentionally returns `408 Request Timeout` about 30% of the time so the configured HTTP retry policy can be exercised.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- One of the following RabbitMQ options:
  - Docker Desktop with Docker Compose
  - RabbitMQ installed with Homebrew on macOS
- JetBrains Rider is optional, but recommended for breakpoint debugging

Verify the .NET installation:

```bash
dotnet --version
```

The repository's `global.json` selects a compatible .NET 8 SDK.

## 1. Start RabbitMQ

Choose either Docker Compose or Homebrew. Do not run both brokers at the same time because they use the same ports.

### Option A: Docker Compose

From the repository root, start only the RabbitMQ service:

```bash
docker compose up -d rabbitmq
docker compose ps rabbitmq
```

The current `compose.yaml` is intended to supply RabbitMQ for local development. Use `docker compose up -d rabbitmq`, rather than starting the entire Compose application, when debugging the API directly in Rider or with `dotnet run`.

To stop it:

```bash
docker compose stop rabbitmq
```

### Option B: Homebrew on macOS

Install and start RabbitMQ as a background service:

```bash
brew install rabbitmq
brew services start rabbitmq
```

Check its status:

```bash
brew services list
/opt/homebrew/opt/rabbitmq/sbin/rabbitmq-diagnostics -q ping
```

To stop it:

```bash
brew services stop rabbitmq
```

### RabbitMQ endpoints

| Purpose | Address | Credentials |
| --- | --- | --- |
| AMQP connection used by the API | `localhost:5672` | `guest` / `guest` |
| Management UI | <http://localhost:15672> | `guest` / `guest` |

The API declares the `partner-transactions` queue automatically on the first successful publish.

## 2. Restore and build

Run these commands from the repository root:

```bash
dotnet restore Partner_Integration_BFF.sln
dotnet build Partner_Integration_BFF.sln --no-restore
```

## 3. Run the API

Use the HTTP development profile:

```bash
dotnet run \
  --project Partner_Integration_BFF/Partner_Integration_BFF.csproj \
  --launch-profile http
```

The API is then available at:

- Swagger UI: <http://localhost:5180/swagger>
- Transaction endpoint: `POST http://localhost:5180/api/v1/partner/transactions`
- Mock partner endpoint: `GET http://localhost:5180/api/mock/partners/{partnerId}`

Stop the API with `Ctrl+C`.

## Debug in Rider

1. Open `Partner_Integration_BFF.sln` in Rider.
2. Select the `Partner_Integration_BFF: http` run configuration.
3. Add a breakpoint in `Controllers/PartnerIntegration.cs` or `Services/PartnerTransactionService.cs`.
4. Click **Debug**.
5. Submit a request through Swagger or an HTTP client.

Keep RabbitMQ running while stepping through a valid transaction. The broker connection is opened only when the publish step is reached.

## Send a test transaction

With the API and RabbitMQ running:

```bash
curl -i -X POST http://localhost:5180/api/v1/partner/transactions \
  -H 'Content-Type: application/json' \
  --data '{
    "partnerId": "P-1001",
    "transactionReference": "TXN-99823",
    "amount": 250,
    "currency": "USD",
    "timestamp": "2026-08-13T14:30:00Z"
  }'
```

The expected response is:

```text
HTTP/1.1 202 Accepted
```

Open the RabbitMQ management UI and select **Queues and Streams** to inspect `partner-transactions`.

Valid requests require:

- A non-empty `partnerId`
- A non-empty `transactionReference`
- An integer `amount` greater than `0`
- A `currency` of `USD`, `EUR`, or `VND`
- A non-empty `timestamp`

## Configuration

The local defaults are:

| Setting | Development value | Purpose |
| --- | --- | --- |
| `RabbitMq:Host` | `localhost` | RabbitMQ host |
| `PartnerVerification:BaseUrl` | `http://localhost:5180/` | Local mock partner API |

ASP.NET Core configuration values can be overridden with double-underscore environment variable names, for example:

```bash
RabbitMq__Host=my-rabbit-host dotnet run \
  --project Partner_Integration_BFF/Partner_Integration_BFF.csproj \
  --launch-profile http
```

## Troubleshooting

### `BrokerUnreachableException: None of the specified endpoints were reachable`

Nothing is accepting AMQP connections at the configured host. Confirm RabbitMQ is running and port `5672` is available:

```bash
nc -vz localhost 5672
```

Then restart the appropriate broker:

```bash
docker compose up -d rabbitmq
```

or:

```bash
brew services restart rabbitmq
```

### `Connection refused (localhost:5001)`

The development configuration was not loaded. Run with `--launch-profile http`, or ensure `ASPNETCORE_ENVIRONMENT` is set to `Development` and the content root is the project directory.

### `dotnet: command not found`

Install the .NET 8 SDK and make sure the `dotnet` executable is on `PATH`. Confirm with `dotnet --info`.

### Port `5180` is already in use

Find the process using the port:

```bash
lsof -nP -iTCP:5180 -sTCP:LISTEN
```

Stop that process or change `applicationUrl` in `Partner_Integration_BFF/Properties/launchSettings.json`.

### Intermittent partner verification timeout

This is expected from the local mock, which randomly returns `408 Request Timeout`. The API retries transient failures. Retry the transaction if all attempts happen to time out.
