using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using UdsTool.Converters;
using UdsTool.Core.Interfaces;
using UdsTool.Services;
using UdsTool.Utils.Converters;
using UdsTool.ViewModels;
using UdsTool.Views;

namespace UdsTool
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        public static new App Current => (App)Application.Current;
        public ServiceProvider ServiceProvider => _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Register services
            services.AddSingleton<IXmlService, XmlService>();
            services.AddSingleton<ICanService, CanService>();
            services.AddSingleton<IUdsService, UdsService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Register view models
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<XmlEditorViewModel>();
            services.AddSingleton<EcuCommunicationViewModel>();
            services.AddSingleton<IsoTpConfigViewModel>();

            // Register views
            services.AddTransient<MainView>();
            services.AddTransient<XmlEditorView>();
            services.AddTransient<EcuCommunicationView>();
            services.AddTransient<IsoTpConfigView>();

            // Register converters for xaml
            services.AddSingleton<InverseBooleanConverter>();
            services.AddSingleton<NullToVisibilityConverter>();
            services.AddSingleton<TypeToVisibilityConverter>();
            services.AddSingleton<StringToHexConverter>();
            services.AddSingleton<ByteArrayToHexStringConverter>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Configure navigation service
            var navigationService = _serviceProvider.GetService<INavigationService>();
            navigationService.RegisterViewModel(_serviceProvider.GetService<XmlEditorViewModel>());
            navigationService.RegisterViewModel(_serviceProvider.GetService<EcuCommunicationViewModel>());
            navigationService.RegisterViewModel(_serviceProvider.GetService<IsoTpConfigViewModel>());

            // Show main window
            var mainWindow = _serviceProvider.GetService<MainView>();
            mainWindow.DataContext = _serviceProvider.GetService<MainViewModel>();
            mainWindow.Show();
        }
    }

    // Helper methods for accessing dependencies
    public T GetService<T>()
        {
            return _serviceProvider.GetService<T>();
        }
    }
}