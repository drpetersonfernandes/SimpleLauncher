using System.Windows;

namespace SimpleLauncher
{
    public partial class EditSystemEasyMode
    {
        public EditSystemEasyMode()
        {
            InitializeComponent();
        }

        private void AddSystemButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            EditSystemEasyModeAddSystem editSystemEasyModeAdd = new();
            Close();
            editSystemEasyModeAdd.ShowDialog();
        }

        private void EditSystemButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            EditSystemEasyModeEditSystem editSystemEasyModeEdit = new();
            Close();
            editSystemEasyModeEdit.ShowDialog();
        }

        private void DeleteSystemButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            EditSystemEasyModeDeleteSystem editSystemEasyModeDelete = new();
            Close();
            editSystemEasyModeDelete.ShowDialog();
        }
    }
}