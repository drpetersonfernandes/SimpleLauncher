using System.Collections.Generic;
using System.Linq;
using System.Windows;
using SimpleLauncher.SharedModels;

namespace SimpleLauncher;

internal partial class GameVerificationWindow
{
    internal List<SelectableGameItem> ConfirmedGames { get; private set; }

    internal GameVerificationWindow(IEnumerable<SelectableGameItem> potentialGames)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        GamesListBox.ItemsSource = potentialGames;
        ConfirmedGames = new List<SelectableGameItem>();
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        if (GamesListBox.ItemsSource is List<SelectableGameItem> items)
        {
            ConfirmedGames = items.Where(static i => i.IsSelected).ToList();
        }

        DialogResult = true;
        Close();
    }
}