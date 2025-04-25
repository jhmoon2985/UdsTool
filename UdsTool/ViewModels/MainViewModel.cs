using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UdsTool.Services;

namespace UdsTool.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private object _currentView;
        private string _title = "UDS Diagnostic Tool";

        public MainViewModel(
            INavigationService navigationService,
            XmlEditorViewModel xmlEditorViewModel,
            EcuCommunicationViewModel ecuCommunicationViewModel)
        {
            _navigationService = navigationService;

            // 네비게이션 이벤트 구독
            _navigationService.NavigationRequested += (sender, viewName) =>
            {
                switch (viewName)
                {
                    case "XmlEditor":
                        CurrentView = xmlEditorViewModel;
                        break;
                    case "EcuCommunication":
                        CurrentView = ecuCommunicationViewModel;
                        break;
                }
            };

            // 명령어 초기화
            ShowXmlEditorCommand = new RelayCommand(_ => _navigationService.NavigateTo("XmlEditor"));
            ShowEcuCommunicationCommand = new RelayCommand(_ => _navigationService.NavigateTo("EcuCommunication"));

            // 기본 화면 설정
            _navigationService.NavigateTo("XmlEditor");
        }

        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public ICommand ShowXmlEditorCommand { get; }
        public ICommand ShowEcuCommunicationCommand { get; }
    }
}
