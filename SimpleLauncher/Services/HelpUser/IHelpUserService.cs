using System.Windows.Controls;

namespace SimpleLauncher.Services.HelpUser;

public interface IHelpUserService
{
    void UpdateHelpUserTextBlock(RichTextBox helpUserRichTextBox, TextBox systemNameTextBox);
}
