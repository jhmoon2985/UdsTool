using System;
using System.Windows.Input;
using UdsTool.Core.Base;
using UdsTool.Core.Interfaces;

namespace UdsTool.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private string _title = "UDS Tool";

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public ICommand NavigateToXmlEditorCommand { get; }
        public ICommand NavigateToEcuCommunicationCommand { get; }

        public MainViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            // Initialize navigation commands
            NavigateToXmlEditorCommand = new RelayCommand(_ => NavigateToXmlEditor());
            NavigateToEcuCommunicationCommand = new RelayCommand(_ => NavigateToEcuCommunication());

            // Set default view
            NavigateToXmlEditor();
        }

        private void NavigateToXmlEditor()
        {
            _navigationService.NavigateTo<XmlEditorViewModel>();
        }

        private void NavigateToEcuCommunication()
        {
            _navigationService.NavigateTo<EcuCommunicationViewModel>();
        }
    }
}