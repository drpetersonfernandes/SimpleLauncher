using SimpleLauncher.Core.Models;

namespace SimpleLauncher.Core.Interfaces;

/// <summary>
///     Sends parameter resolution requests to the remote API and returns the resolved emulator parameters.
/// </summary>
public interface IParameterResolverService
{
    /// <summary>
    ///     Sends a parameter resolution request to the remote API and returns the resolved parameters.
    /// </summary>
    /// <param name="request">The request containing system name, emulator name, and ROM file information.</param>
    /// <returns>The resolved parameters, or null if the API call fails.</returns>
    Task<ParameterResolverResult?> ResolveParametersAsync(ParameterResolverRequest request);
}