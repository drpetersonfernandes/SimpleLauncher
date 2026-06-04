using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using XmlToBinaryConverter.Services;

namespace XmlToBinaryConverter;

public partial class MainWindow
{
    private readonly ConverterService _converterService = new();
    private readonly LogError _logError = new();
    private bool _isConverting;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        var aboutWindow = new AboutWindow
        {
            Owner = this
        };
        aboutWindow.ShowDialog();
    }

    private void ConversionTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        // Check if the controls are initialized yet
        if (InputFilePathTextBox == null || OutputFilePathTextBox == null || StatusMessageTextBlock == null)
            return;

        // Clear paths when switching conversion type
        InputFilePathTextBox.Text = string.Empty;
        OutputFilePathTextBox.Text = string.Empty;
        StatusMessageTextBlock.Text = "Ready to convert";
    }

    private void BrowseInput_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog();
        var conversionType = (ConversionTypeComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString();

        if (conversionType == "XML to Binary")
        {
            dialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
        }
        else
        {
            dialog.Filter = "DAT files (*.dat)|*.dat|All files (*.*)|*.*";
        }

        if (dialog.ShowDialog() != true) return;

        InputFilePathTextBox.Text = dialog.FileName;

        // Suggest an output filename based on input file
        var directory = Path.GetDirectoryName(dialog.FileName);
        var filename = Path.GetFileNameWithoutExtension(dialog.FileName);
        var extension = conversionType == "XML to Binary" ? ".dat" : ".xml";

        if (directory != null)
        {
            OutputFilePathTextBox.Text = Path.Combine(directory, filename + extension);
        }
    }

    private void BrowseOutput_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog();
        var conversionType = (ConversionTypeComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString();

        if (conversionType == "XML to Binary")
        {
            dialog.Filter = "DAT files (*.dat)|*.dat|All files (*.*)|*.*";
            dialog.DefaultExt = ".dat";
        }
        else
        {
            dialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            dialog.DefaultExt = ".xml";
        }

        // Set initial directory and filename if we have input file
        if (!string.IsNullOrEmpty(InputFilePathTextBox.Text))
        {
            dialog.InitialDirectory = Path.GetDirectoryName(InputFilePathTextBox.Text);
            dialog.FileName = Path.GetFileNameWithoutExtension(InputFilePathTextBox.Text) +
                              (conversionType == "XML to Binary" ? ".dat" : ".xml");
        }

        if (dialog.ShowDialog() == true)
        {
            OutputFilePathTextBox.Text = dialog.FileName;
        }
    }

    private async void Convert_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isConverting) return;

            if (string.IsNullOrEmpty(InputFilePathTextBox.Text) ||
                string.IsNullOrEmpty(OutputFilePathTextBox.Text) ||
                !File.Exists(InputFilePathTextBox.Text))
            {
                MessageBox.Show("Please select valid input and output files.", "Invalid Files", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                _isConverting = true;
                ProcessingIndicator.Visibility = Visibility.Visible;
                ConvertButton.IsEnabled = false;

                StatusMessageTextBlock.Text = "Reading XML file...";

                var conversionType = (ConversionTypeComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString();

                // Create a progress reporter to update status
                var progress = new Progress<string>(status => { StatusMessageTextBlock.Text = status; });

                // Start the conversion based on the type
                if (conversionType == "XML to Binary")
                {
                    await _converterService.ConvertXmlToBinary(InputFilePathTextBox.Text, OutputFilePathTextBox.Text, progress);
                }
                else
                {
                    await _converterService.ConvertBinaryToXml(InputFilePathTextBox.Text, OutputFilePathTextBox.Text, progress);
                }

                // Show success message
                MessageBox.Show($"Conversion completed successfully!\nOutput file: {OutputFilePathTextBox.Text}",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessageTextBlock.Text = "Error occurred during conversion";

                // Log the error
                await _logError.LogAsync(ex);

                // Ask user if they want to see the error details
                var result = MessageBox.Show($"An error occurred during conversion:\n{ex.Message}\n\nWould you like to view the detailed error log?",
                    "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);

                if (result == MessageBoxResult.Yes)
                {
                    var errorLog = await _logError.ReadLogAsync();
                    // Show error log in a simple dialog
                    MessageBox.Show(errorLog, "Error Log", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            finally
            {
                _isConverting = false;
                ProcessingIndicator.Visibility = Visibility.Collapsed;
                ConvertButton.IsEnabled = true;
                Mouse.OverrideCursor = null;
            }
        }
        catch (Exception ex)
        {
            _ = _logError.LogAsync(ex);
        }
    }
}
