using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using UdsTool.Commands;
using UdsTool.Services;

namespace UdsTool.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly INavigationService _navigationService;
        private object _currentView;
        private string _currentViewName;

        public MainViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            _navigationService.ViewChanged += (s, e) =>
            {
                CurrentView = e.View;
                CurrentViewName = e.ViewName;
            };

            NavigateToXmlEditorCommand = new RelayCommand(_ => NavigateToXmlEditor());
            NavigateToEcuCommunicationCommand = new RelayCommand(_ => NavigateToEcuCommunication());

            // 초기 화면 설정
            NavigateToXmlEditor();
        }

        public object CurrentView
        {
            get => _currentView;
            set
            {
                if (_currentView != value)
                {
                    _currentView = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CurrentViewName
        {
            get => _currentViewName;
            set
            {
                if (_currentViewName != value)
                {
                    _currentViewName = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsXmlEditorActive => CurrentViewName == "XmlEditor";
        public bool IsEcuCommunicationActive => CurrentViewName == "EcuCommunication";

        public ICommand NavigateToXmlEditorCommand { get; }
        public ICommand NavigateToEcuCommunicationCommand { get; }

        private void NavigateToXmlEditor()
        {
            _navigationService.NavigateTo("XmlEditor");
        }

        private void NavigateToEcuCommunication()
        {
            _navigationService.NavigateTo("EcuCommunication");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            // IsXmlEditorActive와 IsEcuCommunicationActive 속성이 CurrentViewName에 의존하므로
            // CurrentViewName이 변경될 때 이 속성들도 변경 알림을 보냅니다.
            if (propertyName == nameof(CurrentViewName))
            {
                OnPropertyChanged(nameof(IsXmlEditorActive));
                OnPropertyChanged(nameof(IsEcuCommunicationActive));
            }
        }
    }
}