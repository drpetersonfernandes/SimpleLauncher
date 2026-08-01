using System.Windows.Controls;
using SimpleLauncher.Models;

namespace SimpleLauncher.Interfaces;

public interface IDisplaySystemInformation
{
    Task<SystemValidationResult> DisplaySystemInfoAsync(Services.SystemManager.SystemManagerService selectedManager, WrapPanel gameFileGrid, CancellationToken cancellationToken = default);
    SystemValidationResult ValidateSystemConfiguration(Services.SystemManager.SystemManagerService selectedManager);
}
