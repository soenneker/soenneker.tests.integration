[![](https://img.shields.io/nuget/v/soenneker.tests.integration.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.tests.integration/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.tests.integration/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.tests.integration/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.tests.integration.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.tests.integration/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.tests.integration/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.tests.integration/actions/workflows/codeql.yml)

# Soenneker.Tests.Integration

A TUnit base class for HTTP and service-level tests over a shared `WebApplicationFactory` supplied by `IntegrationTestHost`.

## Installation

```bash
dotnet add package Soenneker.Tests.Integration
```

## Define the host

```csharp
using Soenneker.TestHosts.Integration;

public sealed class Host : IntegrationTestHost
{
    public override Task InitializeAsync()
    {
        RegisterFactory<Api.Program>("Api");
        return base.InitializeAsync();
    }
}
```

The project name identifies the appsettings location expected by `IntegrationTestHost`. See that package's README for the output-directory convention.

## Define the tests

```csharp
using Soenneker.Tests.Integration;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class OrdersApiTests : IntegrationTest<Api.Program>
{
    public OrdersApiTests(Host host) : base(host)
    {
    }

    [Test]
    public async Task Gets_orders()
    {
        using HttpResponseMessage response = await Client.GetAsync("/orders");

        await Assert.That(response.IsSuccessStatusCode).IsTrue();
    }
}
```

`Client` is created lazily and disposed with the test instance. It uses the test authentication scheme with `ClientUserId` and `ClientEmail`; it does not add an admin role. Use `Factory.CreateTestHttpClient(...)` directly when a test needs another identity, roles, or a JWT.

## Resolving services

```csharp
OrderRepository repository = Resolve<OrderRepository>(scoped: true);
```

`Resolve<T>()` uses the application's root provider. `Resolve<T>(scoped: true)` creates one async scope per test instance, reuses it, and disposes it during teardown. `Factory` is owned by the shared host and must not be disposed by individual tests.

## Background work and static resolution

`WaitOnQueueToEmpty(cancellationToken)` polls the application's `IQueueInformationUtil` every 500 milliseconds until processing stops. Supply a bounded cancellation token; the default token allows an unhealthy queue to wait indefinitely.

`StaticResolve<T>()` targets the most recently constructed `IntegrationTest<TEntryPoint>` instance. Avoid it in parallel tests because another instance of the same generic test base can replace the static target; prefer the instance `Resolve<T>()` method.

The base class reuses the host's `Faker` and `AutoFaker`, and resolves logging through the test's application scope.
