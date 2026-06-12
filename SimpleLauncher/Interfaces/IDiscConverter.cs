#nullable enable

namespace SimpleLauncher.Interfaces;

public interface IDiscConverter
{
    Task<string?> ConvertChdToIsoAsync(string chdPath);
    Task<string?> ConvertChdToCueBinAsync(string chdPath);
    Task<string?> ConvertPbpToCueBinAsync(string pbpPath);
    Task<string?> ConvertToIsoAsync(string discImagePath);
}
