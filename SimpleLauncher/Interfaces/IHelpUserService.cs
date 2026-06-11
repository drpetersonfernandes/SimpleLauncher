using System.Windows.Controls;

namespace SimpleLauncher.Interfaces;

public interface IHelpUserService
{
    string GetHelpText(string systemName);
    void UpdateHelpUserTextBlock(RichTextBox helpUserRichTextBox, string systemName);
}
