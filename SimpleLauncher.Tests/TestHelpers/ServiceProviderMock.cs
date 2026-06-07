using System.Reflection;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Services.DebugAndBugReport;

namespace SimpleLauncher.Tests.TestHelpers;

/// <summary>
/// Provides a way to mock <see cref="App.ServiceProvider"/> for unit tests.
/// The production code calls <c>App.ServiceProvider.GetRequiredService&lt;ILogErrors&gt;()</c>
/// on failure paths, so a mock is needed to prevent NullReferenceException in tests.
/// </summary>
public static class ServiceProviderMock
{
    private static readonly Lock Lock = new();
    private static IServiceProvider? _originalProvider;

    /// <summary>
    /// Installs a minimal mock <see cref="IServiceProvider"/> into <see cref="App.ServiceProvider"/>
    /// that returns a no-op <see cref="ILogErrors"/> implementation.
    /// </summary>
    /// <param name="configuration">Optional <see cref="IConfiguration"/> to provide to services that request it.</param>
    public static void Install(IConfiguration? configuration = null)
    {
        lock (Lock)
        {
            var property = typeof(App).GetProperty("ServiceProvider", BindingFlags.Public | BindingFlags.Static);
            if (property == null)
            {
                throw new InvalidOperationException("Could not find App.ServiceProvider property via reflection.");
            }

            _originalProvider = property.GetValue(null) as IServiceProvider;

            var mockProvider = new MockServiceProvider(configuration);
            property.SetValue(null, mockProvider);
        }
    }

    /// <summary>
    /// Restores the original <see cref="App.ServiceProvider"/> value.
    /// </summary>
    public static void Restore()
    {
        lock (Lock)
        {
            var property = typeof(App).GetProperty("ServiceProvider", BindingFlags.Public | BindingFlags.Static);
            if (property == null)
            {
                throw new InvalidOperationException("Could not find App.ServiceProvider property via reflection.");
            }

            property.SetValue(null, _originalProvider);
            _originalProvider = null;
        }
    }

    private sealed class MockServiceProvider : IServiceProvider
    {
        private readonly IConfiguration? _configuration;

        public MockServiceProvider(IConfiguration? configuration)
        {
            _configuration = configuration;
        }

        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(ILogErrors))
            {
                return new NoOpLogErrors();
            }

            if (serviceType == typeof(IConfiguration))
            {
                return _configuration;
            }

            return null;
        }
    }

    private sealed class NoOpLogErrors : ILogErrors
    {
        public Task LogErrorAsync(Exception? ex, string? contextMessage = null)
        {
            return Task.CompletedTask;
        }
    }
}
