using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UdsTool.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase _currentViewModel;
        private readonly XmlEditorViewModel _xmlEditorViewModel;
        private readonly EcuCommunicationViewModel _ecuCommunicationViewModel;

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public ICommand ShowXmlEditorCommand { get; }
        public ICommand ShowEcuCommunicationCommand { get; }

        public MainViewModel(XmlEditorViewModel xmlEditorViewModel, EcuCommunicationViewModel ecuCommunicationViewModel)
        {
            _xmlEditorViewModel = xmlEditorViewModel ?? throw new ArgumentNullException(nameof(xmlEditorViewModel));
            _ecuCommunicationViewModel = ecuCommunicationViewModel ?? throw new ArgumentNullException(nameof(ecuCommunicationViewModel));

            CurrentViewModel = _xmlEditorViewModel;

            ShowXmlEditorCommand = new RelayCommand(_ => CurrentViewModel = _xmlEditorViewModel);
            ShowEcuCommunicationCommand = new RelayCommand(_ => CurrentViewModel = _ecuCommunicationViewModel);
        }
    }
}
