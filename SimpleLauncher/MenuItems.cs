using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SimpleLauncher
{
    public class MenuActions
    {
        readonly private Window _window;
        readonly private WrapPanel _zipFileGrid;

        public MenuActions(Window window, WrapPanel zipFileGrid)
        {
            _window = window;
            _zipFileGrid = zipFileGrid;
        }

        public void About_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Simple Launcher.\nAn Open Source Emulator Launcher.\nVersion 1.3", "About");
        }

        public void Exit_Click(object sender, RoutedEventArgs e)
        {
            _window.Close();
        }

        public void HideGames_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in _zipFileGrid.Children)
            {
                if (child is Button btn && btn.Content is Grid grid)
                {
                    var sp = grid.Children.OfType<StackPanel>().FirstOrDefault();
                    if (sp != null)
                    {
                        var image = sp.Children.OfType<Image>().FirstOrDefault();
                        if (image != null && image.Source is BitmapImage bmp && bmp.UriSource.LocalPath.EndsWith("default.png"))
                        {
                            btn.Visibility = Visibility.Collapsed; // hide the button
                        }
                    }
                }
            }
        }

        public void ShowGames_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in _zipFileGrid.Children)
            {
                if (child is Button btn)
                {
                    btn.Visibility = Visibility.Visible; // Show the button
                }
            }
        }
    }

}
