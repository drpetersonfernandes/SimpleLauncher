using System.Collections.Generic;
using System.Linq;
using System.Windows;
using SimpleLauncher.Models.GameScanLogic;

namespace SimpleLauncher;

public partial class GameVerificationWindow
{
    public List<SelectableGameItem> ConfirmedGames { get; private set; }

    public GameVerificationWindow(List<SelectableGameItem> potentialGames)
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
            ConfirmedGames = items.Where(i => i.IsSelected).ToList();
        }

        DialogResult = true;
        Close();
    }
}