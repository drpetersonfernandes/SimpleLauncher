using System;
using System.IO;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;

namespace SimpleLauncher
{
    public partial class OpenPdfFiles
    {
        public OpenPdfFiles()
        {
            InitializeComponent();
            InitializeChromium();
        }

        private void InitializeChromium()
        {
            if (!Cef.IsInitialized)
            {
                var settings = new CefSettings();
                Cef.Initialize(settings);
            }
        }

        public void LoadPdf(string filePath)
        {
            if (File.Exists(filePath))
            {
                var uri = new Uri(filePath);
                Browser.Address = uri.AbsoluteUri;
            }
            else
            {
                MessageBox.Show("PDF file not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}