using System.Windows;
using CreateBatchFilesForPS3Games2.Interfaces;
using CreateBatchFilesForPS3Games2.Services;
using CreateBatchFilesForPS3Games2.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CreateBatchFilesForPS3Games2
{
    public partial class App
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            // Configure DI services
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Register services
            services.AddSingleton<ILogger, Logger>();
            services.AddSingleton<ISfoParser, SfoParser>();
            services.AddSingleton<IBatchFileService, BatchFileService>();

            // Register ViewModels
            services.AddSingleton<MainViewModel>();

            // Register Views
            services.AddSingleton<MainWindow>();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}