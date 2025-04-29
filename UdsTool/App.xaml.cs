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
        private MainView _mainView;

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Services - 싱글톤으로 등록
            services.AddSingleton<IXmlService, XmlService>();
            services.AddSingleton<IEcuCommunicationService, EcuCommunicationService>();
            services.AddSingleton<IDialogService, DialogService>();

            // ViewModels - 싱글톤으로 등록하여 인스턴스 유지
            services.AddSingleton<XmlEditorViewModel>();
            services.AddSingleton<EcuCommunicationViewModel>();
            services.AddTransient<FrameEditDialogViewModel>(); // 다이얼로그는 매번 새로운 인스턴스 필요

            // NavigationService - 싱글톤 ViewModel 참조하도록 수정
            services.AddSingleton<INavigationService>(provider =>
            {
                // 이미 생성된 ViewModel 인스턴스 참조
                var navigationService = new NavigationService(new Dictionary<string, Func<object>>
                {
                    { "XmlEditor", () => provider.GetRequiredService<XmlEditorViewModel>() },
                    { "EcuCommunication", () => provider.GetRequiredService<EcuCommunicationViewModel>() }
                });
                return navigationService;
            });

            // MainViewModel - 싱글톤 NavigationService 참조
            services.AddSingleton<MainViewModel>();

            // Views
            services.AddTransient<MainView>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _mainView = _serviceProvider.GetRequiredService<MainView>();
            _mainView.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
            _mainView.Closing += MainView_Closing;
            _mainView.Show();
        }

        private void MainView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 메인 윈도우가 닫힐 때 저장 확인이 필요한지 확인
            if (_mainView.DataContext is MainViewModel mainViewModel)
            {
                var navigationService = _serviceProvider.GetRequiredService<INavigationService>();

                // 현재 뷰가 XmlEditorViewModel인지 확인
                if (navigationService.CurrentView is XmlEditorViewModel xmlEditorViewModel)
                {
                    // 저장 확인이 취소되었으면 창 닫기도 취소
                    if (!xmlEditorViewModel.PromptSaveIfDirty())
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }
        }
    }
}