using SimpleLauncher.Services.SystemManager;

namespace SimpleLauncher.Interfaces;

public interface ISystemImageResolverService
{
    Task<string> ResolveDisplayImageAsync(SystemManager config);
}
