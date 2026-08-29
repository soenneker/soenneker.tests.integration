[![](https://img.shields.io/nuget/v/soenneker.tests.integration.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.tests.integration/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.tests.integration/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.tests.integration/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.tests.integration.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.tests.integration/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.tests.integration/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.tests.integration/actions/workflows/codeql.yml)

# Soenneker.Tests.Integration

Represents an integration test over a `WebApplicationFactory{TEntryPoint}`.

## Install

```bash
dotnet add package Soenneker.Tests.Integration
```

## Quick start

```csharp
using Soenneker.Tests.Integration.Abstract;

IIntegrationTest<TStartup> integrationTest = /* resolve from DI */;
var result = integrationTest.Resolve();
```

Resolves a service from the application service provider.

## What you get

- `IIntegrationTest<TStartup>` — Represents an integration test over a `WebApplicationFactory{TEntryPoint}`.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `IIntegrationTest<TStartup>.Factory` | The WebApplicationFactory used for creating test HTTP clients. | The WebApplicationFactory used for creating test HTTP clients. |
| `IIntegrationTest<TStartup>.Client` | Fully authenticated admin test client. | Fully authenticated admin test client. |
| `IIntegrationTest<TStartup>.Faker` | Faker instance for generating fake data. | Faker instance for generating fake data. |
| `IIntegrationTest<TStartup>.AutoFaker` | AutoFaker instance for generating auto-populated fake data. | AutoFaker instance for generating auto-populated fake data. |
| `IIntegrationTest<TStartup>.Scope` | The current async service scope used for resolving scoped services. | The current async service scope used for resolving scoped services. |
| `IIntegrationTest<TStartup>.Resolve(scoped)` | Resolves a service from the application service provider. | The resolved service. |
| `IIntegrationTest<TStartup>.WaitOnQueueToEmpty(cancellationToken)` | Waits until the background queue has finished processing all items. | A `ValueTask` representing the asynchronous operation. |
| `IIntegrationTest<TStartup>.CreateScope()` | Creates a new async scope for resolving scoped services. | Returns no value; the requested change is complete when the method returns. |

## Practical notes

- Cancellation stops pending work; it does not undo work that has already completed.
- Dispose instances you own when their scope ends so held resources can be released.
