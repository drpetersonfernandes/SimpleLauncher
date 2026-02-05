using System;
using System.Linq;
using System.Windows;
using SimpleLauncher.Services.RetroAchievements;

namespace SimpleLauncher;

public partial class SystemSelectionWindow
{
    public string SelectedSystem { get; private set; }

    public SystemSelectionWindow(string currentGuess)
    {
        InitializeComponent();
        var systems = RetroAchievementsSystemMatcher.GetSupportedSystemNames();
        SystemComboBox.ItemsSource = systems;

        // Try to pre-select the guess if it exists in the list
        SystemComboBox.SelectedItem = systems.FirstOrDefault(s => s.Equals(currentGuess, StringComparison.OrdinalIgnoreCase));
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        if (SystemComboBox.SelectedItem != null)
        {
            SelectedSystem = SystemComboBox.SelectedItem.ToString();
            DialogResult = true; // This automatically closes the window
        }
    }
}
