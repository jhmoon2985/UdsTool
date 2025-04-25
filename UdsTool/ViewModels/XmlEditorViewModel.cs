using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using UdsTool.Models;
using UdsTool.Services;

namespace UdsTool.ViewModels
{
    public class XmlEditorViewModel : ViewModelBase
    {
        private readonly IXmlService _xmlService;
        private UdsConfiguration _configuration;
        private UdsCommand _selectedCommand;
        private string _xmlContent;
        private string _currentFilePath;
        private bool _isModified;

        public UdsConfiguration Configuration
        {
            get => _configuration;
            set
            {
                if (SetProperty(ref _configuration, value))
                {
                    UpdateXmlContent();
                    OnPropertyChanged(nameof(Commands));
                }
            }
        }

        public ObservableCollection<UdsCommand> Commands =>
            _configuration != null ? new ObservableCollection<UdsCommand>(_configuration.Commands) : new ObservableCollection<UdsCommand>();

        public UdsCommand SelectedCommand
        {
            get => _selectedCommand;
            set => SetProperty(ref _selectedCommand, value);
        }

        public string XmlContent
        {
            get => _xmlContent;
            set
            {
                if (SetProperty(ref _xmlContent, value))
                {
                    IsModified = true;
                }
            }
        }

        public string CurrentFilePath
        {
            get => _currentFilePath;
            set => SetProperty(ref _currentFilePath, value);
        }

        public bool IsModified
        {
            get => _isModified;
            set => SetProperty(ref _isModified, value);
        }

        public ICommand NewConfigurationCommand { get; }
        public ICommand OpenConfigurationCommand { get; }
        public ICommand SaveConfigurationCommand { get; }
        public ICommand SaveAsConfigurationCommand { get; }
        public ICommand AddCommandCommand { get; }
        public ICommand RemoveCommandCommand { get; }
        public ICommand UpdateFromXmlCommand { get; }

        public XmlEditorViewModel(IXmlService xmlService)
        {
            _xmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));

            // Initialize with an empty configuration
            Configuration = new UdsConfiguration();

            // Commands
            NewConfigurationCommand = new RelayCommand(_ => CreateNewConfiguration());
            OpenConfigurationCommand = new RelayCommand(_ => OpenConfigurationAsync());
            SaveConfigurationCommand = new RelayCommand(_ => SaveConfigurationAsync(), _ => IsModified);
            SaveAsConfigurationCommand = new RelayCommand(_ => SaveAsConfigurationAsync());
            AddCommandCommand = new RelayCommand(_ => AddNewCommand());
            RemoveCommandCommand = new RelayCommand(_ => RemoveSelectedCommand(), _ => SelectedCommand != null);
            UpdateFromXmlCommand = new RelayCommand(_ => UpdateFromXml());
        }

        private void CreateNewConfiguration()
        {
            if (IsModified)
            {
                var result = MessageBox.Show("Do you want to save changes before creating a new configuration?",
                    "Unsaved Changes", MessageBoxButton.YesNoCancel);

                if (result == MessageBoxResult.Cancel)
                    return;

                if (result == MessageBoxResult.Yes)
                {
                    SaveConfigurationAsync();
                    if (IsModified) // User canceled save
                        return;
                }
            }

            Configuration = new UdsConfiguration();
            CurrentFilePath = null;
            IsModified = false;
        }

        private async void OpenConfigurationAsync()
        {
            if (IsModified)
            {
                var result = MessageBox.Show("Do you want to save changes before opening another configuration?",
                    "Unsaved Changes", MessageBoxButton.YesNoCancel);

                if (result == MessageBoxResult.Cancel)
                    return;

                if (result == MessageBoxResult.Yes)
                {
                    SaveConfigurationAsync();
                    if (IsModified) // User canceled save
                        return;
                }
            }

            var openFileDialog = new OpenFileDialog
            {
                Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
                Title = "Open UDS Configuration"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var config = await _xmlService.LoadConfigurationAsync(openFileDialog.FileName);
                    Configuration = config;
                    CurrentFilePath = openFileDialog.FileName;
                    IsModified = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading configuration: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void SaveConfigurationAsync()
        {
            if (string.IsNullOrEmpty(CurrentFilePath))
            {
                SaveAsConfigurationAsync();
                return;
            }

            try
            {
                await _xmlService.SaveConfigurationAsync(Configuration, CurrentFilePath);
                IsModified = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving configuration: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SaveAsConfigurationAsync()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
                Title = "Save UDS Configuration"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    await _xmlService.SaveConfigurationAsync(Configuration, saveFileDialog.FileName);
                    CurrentFilePath = saveFileDialog.FileName;
                    IsModified = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving configuration: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddNewCommand()
        {
            var newCommand = new UdsCommand
            {
                Name = $"New Command {Configuration.Commands.Count + 1}",
                ServiceId = 0x22 // Default to ReadDataByIdentifier
            };

            Configuration.Commands.Add(newCommand);
            SelectedCommand = newCommand;
            IsModified = true;
            UpdateXmlContent();
            OnPropertyChanged(nameof(Commands));
        }

        private void RemoveSelectedCommand()
        {
            if (SelectedCommand != null)
            {
                Configuration.Commands.Remove(SelectedCommand);
                SelectedCommand = null;
                IsModified = true;
                UpdateXmlContent();
                OnPropertyChanged(nameof(Commands));
            }
        }

        private void UpdateXmlContent()
        {
            if (Configuration != null)
            {
                try
                {
                    _xmlContent = _xmlService.SerializeToString(Configuration);
                    OnPropertyChanged(nameof(XmlContent));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error serializing configuration: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UpdateFromXml()
        {
            try
            {
                var updatedConfig = _xmlService.DeserializeFromString(XmlContent);
                Configuration = updatedConfig;
                IsModified = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error parsing XML: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
