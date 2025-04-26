using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using UdsTool.Services;
using UdsTool.ViewModels;
using UdsTool.Views;

namespace UdsTool
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Services
            services.AddSingleton<IXmlService, XmlService>();
            services.AddSingleton<IEcuCommunicationService, EcuCommunicationService>();
            services.AddSingleton<INavigationService>(provider =>
            {
                var navigationService = new NavigationService(new Dictionary<string, Func<object>>
                {
                    { "XmlEditor", () => provider.GetRequiredService<XmlEditorViewModel>() },
                    { "EcuCommunication", () => provider.GetRequiredService<EcuCommunicationViewModel>() }
                });
                return navigationService;
            });
            services.AddSingleton<IDialogService, DialogService>();

            // ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<XmlEditorViewModel>();
            services.AddTransient<EcuCommunicationViewModel>();
            services.AddTransient<FrameEditDialogViewModel>();

            // Views
            services.AddTransient<MainView>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainView = _serviceProvider.GetRequiredService<MainView>();
            mainView.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
            mainView.Show();
        }
    }
}