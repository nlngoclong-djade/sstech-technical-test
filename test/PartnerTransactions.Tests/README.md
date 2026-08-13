# PartnerTransactions.Tests

xUnit test project targeting .NET 8.

## What is covered

- Valid transaction request
- Amount <= 0
- Required PartnerId
- Required TransactionReference
- Invalid currency
- Successful Partner Verification API response
- Retry succeeds after transient 408 failures
- Retry stops after the configured maximum attempts

## Setup

1. Put this folder beside your API project:

```text
solution/
├── PartnerTransactions.Api/
└── PartnerTransactions.Tests/
```

2. Update the `ProjectReference` in `PartnerTransactions.Tests.csproj` if your API project uses a different name/path.

3. Because `FluentValidation.TestHelper` is included in the main FluentValidation package in modern versions, make sure the API/test dependency graph contains FluentValidation.

4. The retry tests require `Microsoft.Extensions.Http.Resilience`. If it is not inherited through the API project, add:

```bash
dotnet add PartnerTransactions.Tests package Microsoft.Extensions.Http.Resilience
```

5. Add the project to your solution:

```bash
dotnet sln add PartnerTransactions.Tests/PartnerTransactions.Tests.csproj
```

6. Run tests:

```bash
dotnet test
```

7. Run with coverage:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Namespaces

The sample assumes:

- `PartnerTransactions.Api.Contracts.Requests.PartnerTransactionRequest`
- `PartnerTransactions.Api.Validation.TransactionValidator`
- `PartnerTransactions.Api.Clients.PartnerVerificationClient`

Adjust the `using` statements if your project uses different namespaces.
