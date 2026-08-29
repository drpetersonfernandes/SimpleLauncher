using SimpleLauncher.Core.Models;

namespace SimpleLauncher.Core.Interfaces;

/// <summary>
///     Defines a launch strategy that determines whether it can handle a given launch context and executes the appropriate
///     launch method.
/// </summary>
public interface ILaunchStrategy
{
    /// <summary>
    ///     Gets the priority of the strategy; strategies are evaluated in ascending priority order.
    /// </summary>
    /// <returns>The strategy priority value.</returns>
    int Priority => 100;

    /// <summary>
    ///     Determines whether this strategy can handle the specified launch context.
    /// </summary>
    /// <param name="context">The launch context containing game and emulator details.</param>
    /// <returns>True if this strategy applies to the context; otherwise, false.</returns>
    bool IsMatch(LaunchContext context);

    /// <summary>
    ///     Executes the launch using the specified launcher service.
    /// </summary>
    /// <param name="context">The launch context containing game and emulator details.</param>
    /// <param name="launcher">The launcher service used to perform the actual launch.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ExecuteAsync(LaunchContext context, ILauncherService launcher);
}