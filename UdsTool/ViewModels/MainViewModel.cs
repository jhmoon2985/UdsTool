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

        public MainViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            _navigationService.ViewChanged += (s, e) => CurrentView = e;

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
                _currentView = value;
                OnPropertyChanged();
            }
        }

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
        }
    }
}