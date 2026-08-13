# PartnerTransactions.Tests

xUnit tests for validation, orchestration, controller responses, partner-response handling, and HTTP retry behavior.

All tests are isolated; the API and RabbitMQ do not need to be running. Run commands from the project directory containing `Partner_Integration_BFF.sln`.

## Run tests

```bash
dotnet test Partner_Integration_BFF.sln \
  --logger "console;verbosity=normal"
```

Run one area or method with a fully qualified name filter:

```bash
dotnet test tests/PartnerTransactions.Tests/PartnerTransactions.Tests.csproj \
  --filter "FullyQualifiedName~PartnerTransactions.Tests.Validation"

dotnet test tests/PartnerTransactions.Tests/PartnerTransactions.Tests.csproj \
  --filter "FullyQualifiedName~Post_WhenRequestIsAccepted_ShouldReturnProcessingMessage"
```

List tests without running them:

```bash
dotnet test tests/PartnerTransactions.Tests/PartnerTransactions.Tests.csproj \
  --list-tests
```

## Current coverage

| Area | Cases | Behavior covered |
| --- | ---: | --- |
| Request validation | 11 | Valid input; zero/negative amounts; missing IDs; unsupported currencies |
| Service orchestration | 3 | Invalid and unverified short-circuits; verified publish-once path |
| Controller responses | 3 | `202`, `400`, and `422` mappings and response envelopes |
| Partner verification | 5 | Valid, unsuccessful, invalid-data, missing-data, and mismatched-partner responses |
| HTTP resilience | 2 | Recovery after transient `408` responses and retry exhaustion |
| **Total** | **24** | Theory inputs are counted as individual cases |

The HTTP tests use `TestHelpers/SequenceHttpMessageHandler.cs` to provide deterministic responses without network calls.

## Code coverage

```bash
dotnet test tests/PartnerTransactions.Tests/PartnerTransactions.Tests.csproj \
  --collect:"XPlat Code Coverage"
```

Coverage files are written beneath `tests/PartnerTransactions.Tests/TestResults/`, which is ignored by Git.
