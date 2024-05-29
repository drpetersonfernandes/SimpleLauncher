using System;
using System.IO;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;

namespace SimpleLauncher
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            InitializeChromium();
        }

        private void InitializeChromium()
        {
            if (!Cef.IsInitialized)
            {
                // Define the root cache path
                var rootCachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SimpleLauncher", "CefSharpRootCache");

                // Define the cache path as a child directory of the root cache path
                var cachePath = Path.Combine(rootCachePath, "Cache");

                var settings = new CefSettings
                {
                    // Set the root cache path
                    RootCachePath = rootCachePath,

                    // Set the cache path
                    CachePath = cachePath,

                    // Optionally, suppress the 'Unchecked runtime.lastError' warning
                    LogSeverity = LogSeverity.Disable
                };

                Cef.Initialize(settings);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Cef.Shutdown();
            base.OnExit(e);
        }
    }
}