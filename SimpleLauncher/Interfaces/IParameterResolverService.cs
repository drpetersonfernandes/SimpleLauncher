using SimpleLauncher.Models;

namespace SimpleLauncher.Interfaces;

public interface IParameterResolverService
{
    Task<ParameterResolverResult> ResolveParametersAsync(ParameterResolverRequest request);
}