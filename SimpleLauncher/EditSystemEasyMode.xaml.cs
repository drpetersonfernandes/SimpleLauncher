using System.Windows;

namespace SimpleLauncher
{
    public partial class EditSystemEasyMode
    {
        private readonly SettingsConfig _settings;

        public EditSystemEasyMode(SettingsConfig settings)
        {
            InitializeComponent();
            
            _settings = settings;

            // Apply the theme to this window
            App.ApplyThemeToWindow(this);
        }

        private void AddSystemButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            EditSystemEasyModeAddSystem editSystemEasyModeAdd = new();
            Close();
            editSystemEasyModeAdd.ShowDialog();
        }

        private void EditSystemButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            EditSystem editSystem = new(_settings);
            Close();
            editSystem.ShowDialog();
        }

        private void DeleteSystemButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            EditSystem editSystem = new(_settings);
            Close();
            editSystem.ShowDialog();
        }
    }
}