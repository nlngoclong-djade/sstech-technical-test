# Partner Integration BFF

ASP.NET Core 8 API that validates partner transactions, verifies the partner through an HTTP integration, and publishes accepted transactions to RabbitMQ.

## Architecture

```text
POST /api/v1/partner/transactions
        |
        v
PartnerIntegrationController
        |
        v
PartnerTransactionService
   |              |
   v              v
FluentValidation  VerificationPartner (HTTP + retry/timeout)
                       |
                       v
                RabbitMqMessagePublisher
                       |
                       v
             partner-transactions queue
```

The main design choices are:

- The controller is an HTTP adapter only. It delegates orchestration and maps explicit application outcomes to `202`, `400`, or `422` responses.
- `PartnerTransactionService` owns the validate → verify → publish workflow. Interfaces around partner verification and messaging keep it independently testable.
- FluentValidation keeps request rules outside the controller and prevents invalid transactions from reaching external dependencies.
- A typed `HttpClient` uses the standard resilience handler with three exponential-backoff retries and a three-second attempt timeout.
- RabbitMQ provides the asynchronous boundary. The publisher declares a durable queue, sends persistent messages with mandatory routing, and awaits publisher confirmation before the API returns `202 Accepted`.
- The mock partner endpoint makes the exercise self-contained and deliberately returns `408 Request Timeout` about 30% of the time to exercise retries.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Docker Desktop with Docker Compose, or another RabbitMQ instance

The checked-in `global.json` selects a compatible .NET 8 SDK.

## Run locally

From this directory, start RabbitMQ:

```bash
docker compose up -d rabbitmq
docker compose ps rabbitmq
```

Restore, build, and run the API:

```bash
dotnet restore Partner_Integration_BFF.sln
dotnet build Partner_Integration_BFF.sln --no-restore
dotnet run \
  --project Partner_Integration_BFF/Partner_Integration_BFF.csproj \
  --launch-profile http
```

The local endpoints are:

- Swagger: <http://localhost:5180/swagger>
- Transactions: `POST http://localhost:5180/api/v1/partner/transactions`
- Mock verification: `GET http://localhost:5180/api/mock/partners/{partnerId}`
- RabbitMQ management: <http://localhost:15672> (`guest` / `guest`)

`compose.yaml` intentionally runs only the local broker; the API runs with `dotnet run` so debugging and configuration remain straightforward.

## Example request

```bash
curl -i -X POST http://localhost:5180/api/v1/partner/transactions \
  -H 'Content-Type: application/json' \
  --data '{
    "partnerId": "P-1001",
    "transactionReference": "TXN-99823",
    "amount": 250.00,
    "currency": "USD",
    "timestamp": "2026-08-13T14:30:00Z"
  }'
```

A successful publication returns:

```http
HTTP/1.1 202 Accepted
Content-Type: application/json; charset=utf-8

{
  "isSuccess": true,
  "message": "Transaction request accepted for processing.",
  "statusCode": 202,
  "data": null
}
```

The endpoint returns `400 Bad Request` for invalid transaction data and `422 Unprocessable Entity` when the partner response does not verify the requested partner. A valid request requires non-empty IDs, an amount greater than zero, a currency of `USD`, `EUR`, or `VND`, and a non-default timestamp.

## Run the tests

The 24 xUnit cases are isolated from RabbitMQ and do not require the API to be running:

```bash
dotnet test Partner_Integration_BFF.sln \
  --logger "console;verbosity=normal"
```

Generate coverage with:

```bash
dotnet test tests/PartnerTransactions.Tests/PartnerTransactions.Tests.csproj \
  --collect:"XPlat Code Coverage"
```

The test strategy covers:

- validation rules and invalid boundary values;
- service orchestration, short-circuiting, and publish-once behavior;
- all controller outcome-to-HTTP mappings and response messages;
- partner-response deserialization and consistency checks;
- transient HTTP recovery and retry exhaustion using a deterministic fake handler.

See [tests/PartnerTransactions.Tests/README.md](tests/PartnerTransactions.Tests/README.md) for test filters and the case breakdown.

## Configuration

| Setting | Development default | Purpose |
| --- | --- | --- |
| `RabbitMq:Host` | `localhost` | RabbitMQ host |
| `PartnerVerification:BaseUrl` | `http://localhost:5180/` | Partner verification API |

Override settings with standard ASP.NET Core environment variables, for example:

```bash
RabbitMq__Host=my-rabbit-host \
PartnerVerification__BaseUrl=https://partner.example/ \
dotnet run --project Partner_Integration_BFF/Partner_Integration_BFF.csproj
```

## Trade-offs and production follow-ups

This time-boxed solution favors a small, testable design. For production, the local mock should be replaced or restricted to development, RabbitMQ connections should be reused, and credentials/TLS should be bound through validated options and secret storage. Authentication, rate limiting, idempotency, an outbox, observability, health checks, and integration tests against real dependencies would be the next reliability steps. Publisher confirms and persistent messages improve broker handoff, but they do not by themselves provide exactly-once processing.

Stop local infrastructure with:

```bash
docker compose down
```
