using System;
using System.IO;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;

namespace SimpleLauncher
{
    public partial class OpenPdfFiles : Window
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
                browser.Address = uri.AbsoluteUri;
            }
            else
            {
                MessageBox.Show("PDF file not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            // Do not call Cef.Shutdown() here as it can only be called once per process.
            // Let the application shutdown handle it if necessary.
        }
    }
}