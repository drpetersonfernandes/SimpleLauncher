using System.Windows.Controls;

namespace SimpleLauncher.Interfaces;

/// <summary>
///     Provides methods to display contextual help text for game systems.
/// </summary>
public interface IHelpUserService
{
    /// <summary>
    ///     Gets the help text for the specified system.
    /// </summary>
    /// <param name="systemName">The name of the system to get help text for.</param>
    /// <returns>The help text string.</returns>
    string GetHelpText(string systemName);

    /// <summary>
    ///     Updates the specified RichTextBox with help text for the given system.
    /// </summary>
    /// <param name="helpUserRichTextBox">The RichTextBox control to update.</param>
    /// <param name="systemName">The name of the system to display help for.</param>
    void UpdateHelpUserTextBlock(RichTextBox helpUserRichTextBox, string systemName);
}