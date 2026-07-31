using System.Windows.Controls;
using SimpleLauncher.Models;

namespace SimpleLauncher.Interfaces;

public interface IDisplaySystemInformation
{
    Task<SystemValidationResult> DisplaySystemInfoAsync(Services.SystemManager.SystemManager selectedManager, WrapPanel gameFileGrid, CancellationToken cancellationToken = default);
    SystemValidationResult ValidateSystemConfiguration(Services.SystemManager.SystemManager selectedManager);
}
