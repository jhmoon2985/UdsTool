using System;
using Microsoft.Extensions.DependencyInjection;
using UdsTool.Utils;
using UdsTool.Core.Interfaces;
using UdsTool.Services;
using UdsTool.ViewModels;

namespace UdsTool.DependencyInjection
{
    /// <summary>
    /// Configures dependency injection for the application
    /// </summary>
    public static class DependencyConfig
    {
        private static IServiceProvider _serviceProvider;

        /// <summary>
        /// Configures the service container
        /// </summary>
        public static void ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register services
            services.AddSingleton<IXmlService, XmlService>();
            services.AddSingleton<IUdsService, UdsService>();
            services.AddSingleton<ICanService, CanService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IDialogService, DialogService>();

            // Register view models
            services.AddTransient<MainViewModel>();
            services.AddTransient<XmlEditorViewModel>();
            services.AddTransient<EcuCommunicationViewModel>();
            services.AddTransient<IsoTpConfigViewModel>();

            _serviceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// Gets a service from the container
        /// </summary>
        /// <typeparam name="T">The type of service to retrieve</typeparam>
        /// <returns>The service instance</returns>
        public static T GetService<T>()
        {
            if (_serviceProvider == null)
            {
                ConfigureServices();
            }

            return _serviceProvider.GetRequiredService<T>();
        }
    }

    /// <summary>
    /// Provides access to view models via dependency injection
    /// </summary>
    public class ViewModelLocator
    {
        public MainViewModel MainViewModel => DependencyConfig.GetService<MainViewModel>();
        public XmlEditorViewModel XmlEditorViewModel => DependencyConfig.GetService<XmlEditorViewModel>();
        public EcuCommunicationViewModel EcuCommunicationViewModel => DependencyConfig.GetService<EcuCommunicationViewModel>();
        public IsoTpConfigViewModel IsoTpSettingsViewModel => DependencyConfig.GetService<IsoTpConfigViewModel>();
    }
}