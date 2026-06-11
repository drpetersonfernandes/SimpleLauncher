using System.Windows.Controls;
using SimpleLauncher.Services.DisplaySystemInfo.Models;

namespace SimpleLauncher.Services.DisplaySystemInfo;

public interface IDisplaySystemInformation
{
    Task<SystemValidationResult> DisplaySystemInfoAsync(SystemManager.SystemManager selectedManager, WrapPanel gameFileGrid, CancellationToken cancellationToken = default);
    SystemValidationResult ValidateSystemConfiguration(SystemManager.SystemManager selectedManager);
}
