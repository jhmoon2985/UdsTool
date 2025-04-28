using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using UdsTool.Commands;
using UdsTool.Models;
using UdsTool.Services;
using Microsoft.Win32;
using UdsTool.Views;

namespace UdsTool.ViewModels
{
    public class XmlEditorViewModel : INotifyPropertyChanged
    {
        private readonly IXmlService _xmlService;
        private readonly IDialogService _dialogService;
        private DiagnosticFrame _selectedFrame;
        private string _xmlContent;

        public XmlEditorViewModel(IXmlService xmlService, IDialogService dialogService)
        {
            _xmlService = xmlService;
            _dialogService = dialogService;
            DiagnosticFrames = new ObservableCollection<DiagnosticFrame>();

            AddFrameCommand = new RelayCommand(_ => AddFrame());
            EditFrameCommand = new RelayCommand(_ => EditFrame(), _ => SelectedFrame != null);
            DeleteFrameCommand = new RelayCommand(_ => DeleteFrame(), _ => SelectedFrame != null);
            SaveCommand = new RelayCommand(_ => Save());
            LoadCommand = new RelayCommand(_ => Load());

            UpdateXmlView();
        }

        public ObservableCollection<DiagnosticFrame> DiagnosticFrames { get; set; }

        public DiagnosticFrame SelectedFrame
        {
            get => _selectedFrame;
            set
            {
                _selectedFrame = value;
                OnPropertyChanged();
            }
        }

        public string XmlContent
        {
            get => _xmlContent;
            set
            {
                _xmlContent = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddFrameCommand { get; }
        public ICommand EditFrameCommand { get; }
        public ICommand DeleteFrameCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand LoadCommand { get; }

        private void AddFrame()
        {
            var dialogViewModel = new FrameEditDialogViewModel();

            if (_dialogService.ShowDialog(dialogViewModel) == true)
            {
                var newFrame = dialogViewModel.GetFrame();
                DiagnosticFrames.Add(newFrame);
                SelectedFrame = newFrame;
                UpdateXmlView();
            }
        }

        private void EditFrame()
        {
            if (SelectedFrame != null)
            {
                var dialogViewModel = new FrameEditDialogViewModel(SelectedFrame);

                if (_dialogService.ShowDialog(dialogViewModel) == true)
                {
                    int index = DiagnosticFrames.IndexOf(SelectedFrame);
                    if (index >= 0)
                    {
                        var updatedFrame = dialogViewModel.GetFrame();
                        DiagnosticFrames[index] = updatedFrame;
                        SelectedFrame = updatedFrame;
                    }

                    UpdateXmlView();
                }
            }
        }

        private void DeleteFrame()
        {
            if (SelectedFrame != null)
            {
                DiagnosticFrames.Remove(SelectedFrame);
                UpdateXmlView();
            }
        }

        private void Save()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                DefaultExt = "xml"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                _xmlService.SaveToFile(saveFileDialog.FileName, DiagnosticFrames);
            }
        }

        private void Load()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                DefaultExt = "xml"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var loadedFrames = _xmlService.LoadFromFile(openFileDialog.FileName);
                DiagnosticFrames.Clear();
                foreach (var frame in loadedFrames)
                {
                    DiagnosticFrames.Add(frame);
                }
                UpdateXmlView();
            }
        }

        private void UpdateXmlView()
        {
            try
            {
                XmlContent = _xmlService.SerializeToXml(DiagnosticFrames);
            }
            catch (Exception ex)
            {
                // 오류 처리
                XmlContent = $"Error updating XML view: {ex.Message}";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}