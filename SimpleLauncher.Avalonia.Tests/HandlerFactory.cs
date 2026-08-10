using Microsoft.Extensions.DependencyInjection;
using Moq;
using SimpleLauncher.Avalonia.Services.GameLauncher.Handlers;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
/// Builds any IEmulatorConfigHandler from mocks, matching its constructor by parameter type.
/// </summary>
internal static class HandlerFactory
{
    public static IEmulatorConfigHandler Create<T>() where T : IEmulatorConfigHandler
        => CreateFromType(typeof(T));

    public static IEmulatorConfigHandler CreateFromType(Type handlerType)
    {
        var logger = new Mock<ILogger>().Object;
        var scopeFactory = new Mock<IServiceScopeFactory>().Object;
        var messageBox = new Mock<IMessageBoxLibraryService>().Object;

        var ctor = handlerType.GetConstructors().Single();
        var args = ctor.GetParameters()
            .Select(p => (object)(p.ParameterType switch
            {
                var t when t == typeof(ILogger) => logger,
                var t when t == typeof(IServiceScopeFactory) => scopeFactory,
                var t when t == typeof(IMessageBoxLibraryService) => messageBox,
                _ => throw new InvalidOperationException($"Unhandled handler ctor parameter: {p.ParameterType}")
            }))
            .ToArray();

        return (IEmulatorConfigHandler)ctor.Invoke(args);
    }
}
