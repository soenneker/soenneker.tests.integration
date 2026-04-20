using Bogus;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.Extensions.WebApplicationFactories;
using Soenneker.TestHosts.Integration;
using Soenneker.Tests.Integration.Abstract;
using Soenneker.Tests.Logging;
using Soenneker.Utils.AutoBogus;
using Soenneker.Utils.BackgroundQueue.Abstract;
using System;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Tests.Integration;

/// <inheritdoc cref="IIntegrationTest{TStartup}"/>
public abstract class IntegrationTest<TStartup> : LoggingTest, IIntegrationTest<TStartup> where TStartup : class
{
    private readonly IntegrationTestHost _host;
    private Lazy<HttpClient> _lazyClient = null!;

    private readonly Lazy<IQueueInformationUtil> _queueInformationUtil;

    public WebApplicationFactory<TStartup> Factory => _host.GetFactory<TStartup>().Value;

    public HttpClient Client => _lazyClient.Value;

    public Faker Faker { get; }

    public AutoFaker AutoFaker { get; }

    public AsyncServiceScope? Scope { get; private set; }

    public static IntegrationTest<TStartup>? Instance { get; private set; }

    public const string ClientUserId = "test913b-92d7-4c3e-8f29-5c61c4b9d2fa";
    public const string ClientEmail = "test@example.com";

    protected IntegrationTest(IntegrationTestHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));

        AutoFaker = host.AutoFaker;
        Faker = host.Faker;

        Instance = this;

        _queueInformationUtil = new Lazy<IQueueInformationUtil>(() => Resolve<IQueueInformationUtil>(), LazyThreadSafetyMode.ExecutionAndPublication);

        LazyLogger = new Lazy<ILogger<LoggingTest>>(() => Resolve<ILogger<IntegrationTest<TStartup>>>(scoped: true),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public virtual Task InitializeAsync()
    {
        _lazyClient = new Lazy<HttpClient>(() => Factory.CreateTestHttpClient(ClientUserId, ClientEmail), LazyThreadSafetyMode.ExecutionAndPublication);

        return Task.CompletedTask;
    }

    public T Resolve<T>(bool scoped = false) where T : notnull
    {
        if (!scoped)
            return Factory.Services.GetRequiredService<T>();

        if (Scope is null)
            CreateScope();

        return Scope!.Value.ServiceProvider.GetRequiredService<T>();
    }

    [Pure]
    public static T StaticResolve<T>(bool scoped = false) where T : notnull
    {
        return Instance!.Resolve<T>(scoped);
    }

    public async ValueTask WaitOnQueueToEmpty(CancellationToken cancellationToken = default)
    {
        const int delayMs = 500;

        bool isProcessing;

        do
        {
            isProcessing = await _queueInformationUtil.Value.IsProcessing(cancellationToken).NoSync();

            if (isProcessing)
            {
                await Delay(delayMs, "Background queue emptying...", false, cancellationToken).NoSync();
            }
            else
            {
                Logger.LogDebug("Background queue is empty; continuing");
            }
        }
        while (isProcessing);
    }

    public void CreateScope()
    {
        Scope ??= Factory.Services.CreateAsyncScope();
    }

    public virtual async ValueTask DisposeAsync()
    {
        if (_lazyClient is { IsValueCreated: true })
            _lazyClient.Value.Dispose();

        if (Scope is not null)
        {
            await Scope.Value.DisposeAsync().NoSync();
            Scope = null;
        }

        if (ReferenceEquals(Instance, this))
            Instance = null;
    }
}