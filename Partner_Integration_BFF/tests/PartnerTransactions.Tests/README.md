# PartnerTransactions.Tests

xUnit tests for transaction validation, partner-response handling, and HTTP retry behavior in Partner Integration BFF.

## Prerequisites

- .NET 8 SDK
- Run commands from the repository root:

```bash
cd /Users/djade/Works/MyOwnProject/Partner_Integration_BFF
```

These are isolated unit tests. The API and RabbitMQ do not need to be running.

## Run all tests

The default command prints a concise final summary:

```bash
dotnet test Partner_Integration_BFF.sln
```

Example:

```text
Passed! - Failed: 0, Passed: 15, Skipped: 0, Total: 15
```

To show every test's short scenario-and-reason description followed by the summary, enable the normal console logger:

```bash
dotnet test Partner_Integration_BFF.sln \
  --logger "console;verbosity=normal"
```

Use `verbosity=detailed` when diagnosing a failure. The `--logger` option controls test-result output; `-v` primarily controls build output.

## Run only validator tests

Run all 11 cases for `TransactionValidator`:

```bash
dotnet test tests/PartnerTransactions.Tests/PartnerTransactions.Tests.csproj \
  --filter "FullyQualifiedName~PartnerTransactions.Tests.Validation.TransactionValidatorTests" \
  --logger "console;verbosity=normal"
```

Run one validator method:

```bash
dotnet test tests/PartnerTransactions.Tests/PartnerTransactions.Tests.csproj \
  --filter "FullyQualifiedName~Validate_WhenRequestIsValid_ShouldHaveNoErrors" \
  --logger "console;verbosity=normal"
```

## Current coverage

| Test area | Cases | What and why it verifies |
| --- | ---: | --- |
| Valid transaction | 1 | A complete request passes because every required field satisfies its rule. |
| Invalid amount | 3 | `0`, `-1`, and `-250` fail because the amount must be greater than zero. |
| Missing partner ID | 2 | Empty and null values fail because `partnerId` is required. |
| Missing transaction reference | 2 | Empty and null values fail because `transactionReference` is required. |
| Invalid currency | 3 | Empty, `ABC`, and `US` fail because only `USD`, `EUR`, and `VND` are supported. |
| Valid partner response | 1 | Verification returns true because the response contains `isSuccess: true`. |
| Retry succeeds | 1 | Two transient `408` responses are retried and the third response succeeds. |
| Retry exhausted | 1 | Four `408` responses return the final failure after the initial call and three retries. |
| Accepted controller response | 1 | A `202 Accepted` response includes a processing-status message for the caller. |
| **Total** | **15** | **All current test cases.** |

Theory inputs are counted separately. For example, the invalid-amount theory is one test method but produces three test cases.

## Test source

- `Validation/TransactionValidatorTests.cs` — request validation rules
- `Controllers/PartnerIntegrationControllerTests.cs` — accepted response status and message
- `Clients/PartnerVerificationClientTests.cs` — partner-response deserialization
- `Clients/RetryStrategyTests.cs` — transient timeout and retry behavior
- `TestHelpers/SequenceHttpMessageHandler.cs` — deterministic fake HTTP responses

Each xUnit `DisplayName` states the scenario and reason, so normal console output and Rider's test runner remain easy to scan.

## List tests without running them

```bash
dotnet test tests/PartnerTransactions.Tests/PartnerTransactions.Tests.csproj \
  --list-tests
```

## Code coverage

```bash
dotnet test tests/PartnerTransactions.Tests/PartnerTransactions.Tests.csproj \
  --collect:"XPlat Code Coverage"
```

Coverage files are written under `tests/PartnerTransactions.Tests/TestResults/`.

## Troubleshooting

### Only the final `Passed!` line is shown

This is normal for the default console logger. Add:

```bash
--logger "console;verbosity=normal"
```

### `dotnet: command not found`

Install the .NET 8 SDK or invoke the local SDK directly:

```bash
/Users/djade/.dotnet/dotnet test Partner_Integration_BFF.sln \
  --logger "console;verbosity=normal"
```

### No tests match the filter

Run `--list-tests`, then compare the namespace and method name with the filter. Filters are case-sensitive in some environments.

### Dependencies have not been restored

```bash
dotnet restore Partner_Integration_BFF.sln
dotnet test Partner_Integration_BFF.sln --no-restore
```
