using Microsoft.Extensions.DependencyInjection;
using System;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Navigation;
using UdsTool.Services;
using UdsTool.ViewModels;
using UdsTool.Views;

namespace UdsTool;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider _serviceProvider;

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(ServiceCollection services)
    {
        // Register services
        services.AddSingleton<INavigationService, Services.NavigationService>();
        services.AddSingleton<IXmlService, XmlService>();
        services.AddSingleton<IEcuCommunicationService, EcuCommunicationService>();

        // Register view models
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<XmlEditorViewModel>();
        services.AddSingleton<EcuCommunicationViewModel>();
        services.AddSingleton<IsoTpSettingsViewModel>();

        // Register Main Window
        services.AddSingleton<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}

