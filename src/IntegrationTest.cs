using Bogus;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Sinks.XUnit.Injectable.Abstract;
using Soenneker.Extensions.ServiceProvider;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.Extensions.WebApplicationFactories;
using Soenneker.Fixtures.Integration;
using Soenneker.Tests.Integration.Abstract;
using Soenneker.Tests.Logging;
using Soenneker.Utils.AutoBogus;
using Soenneker.Utils.BackgroundQueue.Abstract;
using System;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Soenneker.Tests.Integration;

///<inheritdoc cref="IIntegrationTest{TStartup}"/>
public abstract class IntegrationTest<TStartup> : LoggingTest, IAsyncLifetime, IIntegrationTest<TStartup> where TStartup : class
{
    public WebApplicationFactory<TStartup> Factory => _fixture.GetFactory<TStartup>().Value;

    public HttpClient Client => _lazyClient.Value;

    private Lazy<HttpClient> _lazyClient = null!;

    private readonly IntegrationFixture _fixture;

    public Faker Faker { get; }

    public AutoFaker AutoFaker { get; }

    public AsyncServiceScope? Scope { get; set; }

    public static IntegrationTest<TStartup>? Instance { get; set; }

    private readonly Lazy<IQueueInformationUtil> _queueInformationUtil;

    public const string ClientUserId = "test913b-92d7-4c3e-8f29-5c61c4b9d2fa";
    public const string ClientEmail = "test@example.com";

    protected IntegrationTest(IntegrationFixture fixture, ITestOutputHelper testOutputHelper)
    {
        _fixture = fixture;
        AutoFaker = fixture.AutoFaker;
        Faker = AutoFaker.Faker;

        // IntegrationTest should not own this sink
        var outputSink = Resolve<IInjectableTestOutputSink>();
        outputSink.Inject(testOutputHelper);

        Instance = this;
        _queueInformationUtil = new Lazy<IQueueInformationUtil>(() => Resolve<IQueueInformationUtil>(), LazyThreadSafetyMode.ExecutionAndPublication);

        LazyLogger = new Lazy<ILogger<LoggingTest>>(() => Resolve<ILogger<IntegrationTest<TStartup>>>(true), LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public ValueTask InitializeAsync()
    {
        _lazyClient = new Lazy<HttpClient>(() => Factory.CreateTestHttpClient(ClientUserId, ClientEmail), true);
        return ValueTask.CompletedTask;
    }

    public T Resolve<T>(bool scoped = false)
    {
        if (!scoped)
            return Factory.Services.Get<T>();

        if (Scope == null)
            CreateScope();

        return Scope!.Value.ServiceProvider.Get<T>();
    }

    [Pure]
    public static T StaticResolve<T>(bool scoped = false)
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
                await Delay(delayMs, "Background queue emptying...", false).NoSync();
            }
            else
            {
                Logger.LogDebug("Background queue is empty; continuing");
            }
        } while (isProcessing);
    }

    public void CreateScope()
    {
        Scope = Factory.Services.CreateAsyncScope();
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        if (_lazyClient is { IsValueCreated: true })
            _lazyClient.Value.Dispose();

        if (Scope != null)
            await Scope.Value.DisposeAsync().NoSync();

        Instance = null;
    }
}
