using System.Windows;
using System.Reflection;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace SimpleLauncher;

public partial class BugReport
{
    private static readonly HttpClient HttpClient = new();
    private static string ApiKey { get; set; }
        
    public BugReport()
    {
        InitializeComponent();

        App.ApplyThemeToWindow(this);
            
        // Set the data context for data binding
        DataContext = this; 
            
        // Load the API key
        LoadConfiguration(); 
    }
        
    private void LoadConfiguration()
    {
        string configFile = "appsettings.json";
        if (File.Exists(configFile))
        {
            var config = JObject.Parse(File.ReadAllText(configFile));
            ApiKey = config[nameof(ApiKey)]?.ToString();
        }
        else
        {
            string formattedException = $"File appsettings.json is missing in the Bug Report Window.";
            Exception exception = new(formattedException);
            Task logTask = LogErrors.LogErrorAsync(exception, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            // Ask the user if they want to automatically reinstall Simple Launcher
            var messageBoxResult = MessageBox.Show(
                "File appsettings.json is missing.\n\n" +
                "The application will not be able to send the Bug Report.\n\n" +
                "Do you want to automatically reinstall Simple Launcher to fix the problem?",
                "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                Loaded += async (_, _) => await UpdateChecker.ReinstallSimpleLauncherAsync(this);
            }
            else
            {
                MessageBox.Show("Please reinstall Simple Launcher to fix the problem.",
                    "Warning", MessageBoxButton.OK,MessageBoxImage.Warning);
            }
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private static string ApplicationVersion
    {
        get
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return "Version: " + (version?.ToString() ?? "Unknown");
        }
    }

    private async void SendBugReport_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string nameText = NameTextBox.Text;
            string emailText = EmailTextBox.Text;
            string bugReportText = BugReportTextBox.Text;
            string applicationVersion = ApplicationVersion;

            if (string.IsNullOrWhiteSpace(bugReportText))
            {
                string pleaseenterthedetailsofthebug2 = (string)Application.Current.TryFindResource("Pleaseenterthedetailsofthebug") ?? "Please enter the details of the bug.";
                string info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
                MessageBox.Show(pleaseenterthedetailsofthebug2, info2, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string fullMessage = $"\n\n{applicationVersion}\n" +
                                 $"Name: {nameText}\n" +
                                 $"Email: {emailText}\n" +
                                 $"Bug Report:\n\n{bugReportText}";
            await SendBugReportToApiAsync(fullMessage);
        }
        catch (Exception ex)
        {
            string formattedException = $"Error in the SendBugReport_Click method.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);            }
    }

    private async Task SendBugReportToApiAsync(string fullMessage)
    {
        string messageWithVersion = fullMessage;

        // Prepare the POST data
        var formData = new MultipartFormDataContent
        {
            { new StringContent("contact@purelogiccode.com"), "recipient" },
            { new StringContent("Bug Report from SimpleLauncher"), "subject" },
            { new StringContent("SimpleLauncher User"), "name" },
            { new StringContent(messageWithVersion), "message" }
        };

        // Set the API Key from the loaded configuration
        HttpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        if (!string.IsNullOrEmpty(ApiKey))
        {
            HttpClient.DefaultRequestHeaders.Add("X-API-KEY", ApiKey);
        }
        else
        {
            string formattedException = $"API Key is not properly loaded from appsettings.json in the Bug Report Window.";
            Exception exception = new(formattedException);
            await LogErrors.LogErrorAsync(exception, formattedException);
                
            MessageBox.Show("There was an error in the API Key of this form.\n\n" +
                            "The developer was informed and will try to fix the issue.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        try
        {
            HttpResponseMessage response = await HttpClient.PostAsync("https://purelogiccode.com/simplelauncher/send_email.php", formData);

            if (response.IsSuccessStatusCode)
            {
                string bugreportsent2 = (string)Application.Current.TryFindResource("Bugreportsent") ?? "Bug report sent successfully.";
                string success2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
                MessageBox.Show(bugreportsent2, success2, MessageBoxButton.OK, MessageBoxImage.Information);
                NameTextBox.Clear();
                EmailTextBox.Clear();
                BugReportTextBox.Clear();
            }
            else
            {
                string errorMessage = "An error occurred while sending the bug report.";
                Exception exception = new Exception(errorMessage);
                await LogErrors.LogErrorAsync(exception, errorMessage);
                    
                MessageBox.Show("An error occurred while sending the bug report.\n\n" +
                                "The bug was reported to the developer that will try to fix the issue.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            await LogErrors.LogErrorAsync(ex, $"Error sending the bug report from Bug Report Window.\n\n" +
                                              $"Exception type: {ex.GetType().Name}\n" +
                                              $"Exception details: {ex.Message}");
                
            MessageBox.Show($"An error occurred while sending the bug report.\n\n" +
                            $"The bug was reported to the developer that will try to fix the issue.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

    }
}