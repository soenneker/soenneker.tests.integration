using Bogus;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Soenneker.Utils.AutoBogus;
using System;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TUnit.Core.Interfaces;

namespace Soenneker.Tests.Integration.Abstract;

/// <summary>
/// Represents an integration test over a <see cref="WebApplicationFactory{TEntryPoint}"/>.
/// </summary>
/// <typeparam name="TStartup">The startup or entry point type for the application under test.</typeparam>
public interface IIntegrationTest<TStartup> : IAsyncInitializer, IAsyncDisposable where TStartup : class
{
    /// <summary>
    /// The WebApplicationFactory used for creating test HTTP clients.
    /// </summary>
    WebApplicationFactory<TStartup> Factory { get; }

    /// <summary>
    /// Fully authenticated admin test client.
    /// </summary>
    HttpClient Client { get; }

    /// <summary>
    /// Faker instance for generating fake data.
    /// </summary>
    Faker Faker { get; }

    /// <summary>
    /// AutoFaker instance for generating auto-populated fake data.
    /// </summary>
    AutoFaker AutoFaker { get; }

    /// <summary>
    /// The current async service scope used for resolving scoped services.
    /// </summary>
    AsyncServiceScope? Scope { get; }

    /// <summary>
    /// Resolves a service from the application service provider.
    /// </summary>
    /// <typeparam name="T">The type of service to resolve.</typeparam>
    /// <param name="scoped">If true, resolves from a scoped provider.</param>
    /// <returns>The resolved service.</returns>
    [Pure]
    T Resolve<T>(bool scoped = false) where T : notnull;

    /// <summary>
    /// Waits until the background queue has finished processing all items.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask WaitOnQueueToEmpty(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new async scope for resolving scoped services.
    /// </summary>
    void CreateScope();
}