using MAMEUtility;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Xml.Linq;

namespace MameUtility
{
    public partial class MainWindow
    {
        private readonly BackgroundWorker _worker;
        private readonly LogWindow _logWindow;

        public MainWindow()
        {
            InitializeComponent();

            _worker = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            _worker.ProgressChanged += Worker_ProgressChanged;

            _logWindow = new LogWindow();
            _logWindow.Show();

        }

        private void Log(string message)
        {
            _logWindow.AppendLog(message);
        }

        private void DonateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://www.buymeacoffee.com/purelogiccode",
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Unable to open the link: " + ex.Message);
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new();
            aboutWindow.ShowDialog();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private async void CreateMAMEFull_Click(object sender, RoutedEventArgs e)
        {
            _worker.ReportProgress(0);

            Log("Select MAME full driver information in XML. You can download this file from the MAME Website.");
            Microsoft.Win32.OpenFileDialog openFileDialog = new()
            {
                Title = "Select MAME full driver information in XML",
                Filter = "XML files (*.xml)|*.xml"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var inputFilePath = openFileDialog.FileName;

                Log("Put a name to your output file.");
                Microsoft.Win32.SaveFileDialog saveFileDialog = new()
                {
                    Title = "Save MAMEFull",
                    Filter = "XML files (*.xml)|*.xml",
                    FileName = "MAMEFull.xml"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var outputFilePathMameFull = saveFileDialog.FileName;

                    try
                    {
                        var inputDoc = XDocument.Load(inputFilePath);
                        await MameFull.CreateAndSaveMameFullAsync(inputDoc, outputFilePathMameFull, _worker);
                        Log("Output file saved.");
                    }
                    catch (Exception ex)
                    {
                        Log("An error occurred: " + ex.Message);
                    }
                }
                else
                {
                    Log("No output file specified for MAMEFull.xml. Operation cancelled.");
                }
            }
            else
            {
                Log("No input file selected. Operation cancelled.");
            }
        }

        private async void CreateMAMEManufacturer_Click(object sender, RoutedEventArgs e)
        {
            _worker.ReportProgress(0);

            Log("Select MAME full driver information in XML. You can download this file from the MAME Website.");
            Microsoft.Win32.OpenFileDialog openFileDialog = new()
            {
                Title = "Select MAME full driver information in XML",
                Filter = "XML files (*.xml)|*.xml"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var inputFilePath = openFileDialog.FileName;

                Log("Select Output Folder.");
                var folderBrowserDialog = new FolderBrowserDialog
                {
                    Description = "Select Output Folder"
                };

                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var outputFolderMameManufacturer = folderBrowserDialog.SelectedPath;

                    try
                    {
                        var inputDoc = XDocument.Load(inputFilePath);

                        var progress = new Progress<int>(value =>
                        {
                            ProgressBar.Value = value;
                        });

                        await MAMEManufacturer.CreateAndSaveMameManufacturerAsync(inputDoc, outputFolderMameManufacturer, progress);
                        Log("Data extracted and saved successfully for all manufacturers.");
                    }
                    catch (Exception ex)
                    {
                        Log("An error occurred: " + ex.Message);
                    }
                }
                else
                {
                    Log("No output folder specified. Operation cancelled.");
                }
            }
            else
            {
                Log("No input file selected. Operation cancelled.");
            }
        }

        private async void CreateMAMEYear_Click(object sender, RoutedEventArgs e)
        {
            _worker.ReportProgress(0);

            Log("Select MAME full driver information in XML. You can download this file from the MAME Website.");
            Microsoft.Win32.OpenFileDialog openFileDialog = new()
            {
                Title = "Select MAME full driver information in XML",
                Filter = "XML files (*.xml)|*.xml"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var inputFilePath = openFileDialog.FileName;

                Log("Select Output Folder.");
                var folderBrowserDialog = new FolderBrowserDialog
                {
                    Description = "Select Output Folder"
                };

                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var outputFolderMameYear = folderBrowserDialog.SelectedPath;

                    try
                    {
                        var inputDoc = XDocument.Load(inputFilePath);

                        var progress = new Progress<int>(value =>
                        {
                            ProgressBar.Value = value;
                        });

                        await Task.Run(() => MameYear.CreateAndSaveMameYear(inputDoc, outputFolderMameYear, progress));
                        Log("XML files created successfully for all years.");
                    }
                    catch (Exception ex)
                    {
                        Log("An error occurred: " + ex.Message);
                    }
                }
                else
                {
                    Log("No output folder specified. Operation cancelled.");
                }
            }
            else
            {
                Log("No input file selected. Operation cancelled.");
            }
        }

        private async void CreateMAMESourcefile_Click(object sender, RoutedEventArgs e)
        {
            _worker.ReportProgress(0);

            Log("Select MAME full driver information in XML. You can download this file from the MAME Website.");
            Microsoft.Win32.OpenFileDialog openFileDialog = new()
            {
                Title = "Select MAME full driver information in XML",
                Filter = "XML files (*.xml)|*.xml"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var inputFilePath = openFileDialog.FileName;

                Log("Select Output Folder.");
                FolderBrowserDialog folderBrowserDialog = new()
                {
                    Description = "Select Output Folder"
                };

                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var outputFolderMameSourcefile = folderBrowserDialog.SelectedPath;

                    try
                    {
                        var inputDoc = XDocument.Load(inputFilePath);

                        var progress = new Progress<int>(value =>
                        {
                            ProgressBar.Value = value;
                        });

                        await MameSourcefile.CreateAndSaveMameSourcefileAsync(inputDoc, outputFolderMameSourcefile, progress);
                        Log("Data extracted and saved successfully for all source files.");
                    }
                    catch (Exception ex)
                    {
                        Log("An error occurred: " + ex.Message);
                    }
                }
                else
                {
                    Log("No output folder specified. Operation cancelled.");
                }
            }
            else
            {
                Log("No input file selected. Operation cancelled.");
            }
        }

        private void MergeLists_Click(object sender, RoutedEventArgs e)
        {
            _worker.ReportProgress(0);

            Log("Select XML files to merge. You can select multiple XML files.");
            Microsoft.Win32.OpenFileDialog openFileDialog = new()
            {
                Title = "Select XML files to merge",
                Filter = "XML files (*.xml)|*.xml",
                Multiselect = true // Enable multiple file selection
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string[] inputFilePaths = openFileDialog.FileNames; // Get all selected file paths

                Log("Select where to save the merged XML file.");
                Microsoft.Win32.SaveFileDialog saveFileDialog = new()
                {
                    Title = "Save Merged XML",
                    Filter = "XML files (*.xml)|*.xml",
                    FileName = "Merged.xml"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var outputXmlPath = saveFileDialog.FileName;
            
                    // Create DAT filename based on XML filename (replace extension)
                    var outputDatPath = Path.ChangeExtension(outputXmlPath, ".dat");

                    try
                    {
                        // Use the new method that creates both XML and DAT files
                        MergeList.MergeAndSaveBoth(inputFilePaths, outputXmlPath, outputDatPath);
                        Log($"Merging completed. Created XML file ({outputXmlPath}) and DAT file ({outputDatPath}).");

                        _worker.ReportProgress(100);
                    }
                    catch (Exception ex)
                    {
                        Log("An error occurred: " + ex.Message);
                    }
                }
                else
                {
                    Log("No output file specified for merged XML. Operation cancelled.");
                }
            }
            else
            {
                Log("No input file selected. Operation cancelled.");
            }
        }

        private async void CopyRoms_Click(object sender, RoutedEventArgs e)
        {
            _worker.ReportProgress(0);

            Log("Select the source directory containing the ROMs.");
            var sourceFolderBrowserDialog = new FolderBrowserDialog
            {
                Description = "Select the source directory containing the ROMs"
            };

            if (sourceFolderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var sourceDirectory = sourceFolderBrowserDialog.SelectedPath;

                Log("Select the destination directory for the ROMs.");
                var destinationFolderBrowserDialog = new FolderBrowserDialog
                {
                    Description = "Select the destination directory for the ROMs"
                };

                if (destinationFolderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var destinationDirectory = destinationFolderBrowserDialog.SelectedPath;

                    Log("Please select the XML file(s) containing ROM information. You can select multiple XML files.");
                    Microsoft.Win32.OpenFileDialog openFileDialog = new()
                    {
                        Title = "Please select the XML file(s) containing ROM information",
                        Filter = "XML Files (*.xml)|*.xml",
                        Multiselect = true
                    };

                    if (openFileDialog.ShowDialog() == true)
                    {
                        string[] xmlFilePaths = openFileDialog.FileNames;

                        try
                        {
                            var progress = new Progress<int>(value =>
                            {
                                ProgressBar.Value = value;
                            });

                            await CopyRoms.CopyRomsFromXmlAsync(xmlFilePaths, sourceDirectory, destinationDirectory, progress);
                            Log("ROM copy operation is finished.");
                        }
                        catch (Exception ex)
                        {
                            Log($"An error occurred: {ex.Message}");
                        }

                        _worker.ReportProgress(100);
                    }
                    else
                    {
                        Log("You did not provide the XML file(s) containing ROM information. Operation cancelled.");
                    }
                }
                else
                {
                    Log("You did not select a destination directory for the ROMs. Operation cancelled.");
                }
            }
            else
            {
                Log("You did not provide the source directory containing the ROMs. Operation cancelled.");
            }
        }

        private async void CopyImages_Click(object sender, RoutedEventArgs e)
        {
            _worker.ReportProgress(0);

            Log("Select the source directory containing the images.");
            var sourceFolderBrowserDialog = new FolderBrowserDialog
            {
                Description = "Select the source directory containing the images"
            };

            if (sourceFolderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var sourceDirectory = sourceFolderBrowserDialog.SelectedPath;

                Log("Select the destination directory for the images.");
                var destinationFolderBrowserDialog = new FolderBrowserDialog
                {
                    Description = "Select the destination directory for the images"
                };

                if (destinationFolderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var destinationDirectory = destinationFolderBrowserDialog.SelectedPath;

                    Log("Please select the XML file(s) containing ROM information. You can select multiple XML files.");
                    Microsoft.Win32.OpenFileDialog openFileDialog = new()
                    {
                        Title = "Please select the XML file(s) containing ROM information",
                        Filter = "XML Files (*.xml)|*.xml",
                        Multiselect = true
                    };

                    if (openFileDialog.ShowDialog() == true)
                    {
                        string[] xmlFilePaths = openFileDialog.FileNames;

                        try
                        {
                            var progressReporter = new ProgressBarProgressReporter(_worker);

                            await CopyImages.CopyImagesFromXmlAsync(xmlFilePaths, sourceDirectory, destinationDirectory, progressReporter);
                            Log("Image copy operation is finished.");
                        }
                        catch (Exception ex)
                        {
                            Log("An error occurred: " + ex.Message);
                        }
                    }
                }
                else
                {
                    Log("No destination directory selected. Operation cancelled.");
                }
            }
            else
            {
                Log("No source directory selected. Operation cancelled.");
            }
        }

        private void Worker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            Dispatcher.Invoke(() => { ProgressBar.Value = e.ProgressPercentage; });
        }

        public class ProgressBarProgressReporter(BackgroundWorker worker) : IProgress<int>
        {
            private readonly BackgroundWorker _worker = worker;

            public void Report(int value)
            {
                _worker.ReportProgress(value);
            }
        }

        private void CreateMAMESoftwareList_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Select the folder containing XML files to process.");
            using var folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Select the folder containing XML files to process";

            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var inputFolderPath = folderBrowserDialog.SelectedPath;

                Console.WriteLine("Choose a location to save the consolidated output XML file.");
                Microsoft.Win32.SaveFileDialog saveFileDialog = new()
                {
                    Title = "Save Consolidated XML File",
                    Filter = "XML Files (*.xml)|*.xml",
                    FileName = "MAMESoftwareList.xml"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var outputFilePath = saveFileDialog.FileName;

                    try
                    {
                        var progress = new Progress<int>(value =>
                        {
                            ProgressBar.Value = value;
                        });

                        MameSoftwareList.CreateAndSaveSoftwareList(inputFolderPath, outputFilePath, progress, _logWindow);
                        Console.WriteLine("Consolidated XML file created successfully.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("An error occurred: " + ex.Message);
                    }
                }
                else
                {
                    Console.WriteLine("No output file specified. Operation cancelled.");
                }
            }
            else
            {
                Console.WriteLine("No folder selected. Operation cancelled.");
            }
        }

    }
}