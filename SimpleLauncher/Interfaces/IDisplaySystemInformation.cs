using System.Windows.Controls;
using SimpleLauncher.Models;

namespace SimpleLauncher.Interfaces;

/// <summary>
/// Provides methods to display and validate system configuration information.
/// </summary>
public interface IDisplaySystemInformation
{
    /// <summary>
    /// Asynchronously displays system information for the selected system manager in the specified UI panel.
    /// </summary>
    /// <param name="selectedManager">The system manager whose information to display.</param>
    /// <param name="gameFileGrid">The WrapPanel used to display the information.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The result of the system validation.</returns>
    Task<SystemValidationResult> DisplaySystemInfoAsync(Services.SystemManager.SystemManagerService selectedManager, WrapPanel gameFileGrid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the system configuration without displaying any UI.
    /// </summary>
    /// <param name="selectedManager">The system manager to validate.</param>
    /// <returns>The result of the system validation.</returns>
    SystemValidationResult ValidateSystemConfiguration(Services.SystemManager.SystemManagerService selectedManager);
}
