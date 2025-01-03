using System;
using System.IO;
using System.Threading.Tasks;

namespace SimpleLauncher
{
    public partial class UpdateHistory
    {
        public UpdateHistory()
        {
            InitializeComponent();
            LoadWhatsNewContent();
        }

        private void LoadWhatsNewContent()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "whatsnew.txt");

            try
            {
                WhatsNewTextBox.Text = File.Exists(filePath) ? File.ReadAllText(filePath) : "whatsnew.txt file not found in the application folder.";
            }
            catch (Exception ex)
            {
                string formattedException = $"whatsnew.txt file not found or could not be loaded in the UpdateHistory window.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                logTask.Wait(TimeSpan.FromSeconds(2));
            }
            
        }
    }
}