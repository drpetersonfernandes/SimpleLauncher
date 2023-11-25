using System.Windows;
using System.Windows.Controls;

namespace SimpleLauncher
{
    public class MenuActions(Window window, WrapPanel gameFileGrid)
    {
        readonly private Window _window = window;
        readonly private WrapPanel _gameFileGrid = gameFileGrid;

        public void About_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Simple Launcher.\nAn Open Source Emulator Launcher.\nVersion 2.2", "About");
        }

        public void Exit_Click(object sender, RoutedEventArgs e)
        {
            _window.Close();
        }

        public void HideGames_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in _gameFileGrid.Children)
            {
                if (child is Button btn && btn.Tag?.ToString() == "DefaultImage")
                {
                    btn.Visibility = Visibility.Collapsed; // Hide the button
                }
            }
        }

        public void ShowGames_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in _gameFileGrid.Children)
            {
                if (child is Button btn)
                {
                    btn.Visibility = Visibility.Visible; // Show the button
                }
            }
        }
    }

}
