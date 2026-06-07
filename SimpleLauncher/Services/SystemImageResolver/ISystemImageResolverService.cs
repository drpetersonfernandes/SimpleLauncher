namespace SimpleLauncher.Services.SystemImageResolver;

public interface ISystemImageResolverService
{
    Task<string> ResolveDisplayImageAsync(SystemManager.SystemManager config);
}
