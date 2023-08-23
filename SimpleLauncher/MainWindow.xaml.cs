using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace SimpleLauncher
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadZipFiles();
        }

        private void LoadZipFiles()
        {
            try
            {
                // Get the directory where the application is running
                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

                // Get all the zip files from the current directory
                List<string> zipFiles = Directory.GetFiles(currentDirectory, "*.zip").ToList();

                // Sort the list in alphabetical order
                zipFiles.Sort();

                // Extract only file names from the full path and update the ListBox
                zipFileList.ItemsSource = zipFiles.Select(filePath => Path.GetFileNameWithoutExtension(filePath));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
    }
}