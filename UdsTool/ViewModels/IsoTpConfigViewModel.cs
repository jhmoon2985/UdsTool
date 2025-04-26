using Microsoft.Win32;
using System;
using System.Windows.Input;
using UdsTool.Core.Base;
using UdsTool.Core.Interfaces;
using UdsTool.Core.Models;

namespace UdsTool.ViewModels
{
    public class IsoTpConfigViewModel : ViewModelBase
    {
        private readonly IXmlService _xmlService;
        private readonly IUdsService _udsService;
        private IsoTpConfig _config;
        private bool _isModified;

        public IsoTpConfig Config
        {
            get => _config;
            set
            {
                if (SetProperty(ref _config, value))
                {
                    _udsService.Configuration = value;
                    IsModified = true;
                }
            }
        }

        public bool IsModified
        {
            get => _isModified;
            set => SetProperty(ref _isModified, value);
        }

        public ICommand SaveConfigCommand { get; }
        public ICommand LoadConfigCommand { get; }
        public ICommand ResetToDefaultsCommand { get; }
        public ICommand ApplyConfigCommand { get; }

        public IsoTpConfigViewModel(IXmlService xmlService, IUdsService udsService)
        {
            _xmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));
            _udsService = udsService ?? throw new ArgumentNullException(nameof(udsService));

            // Initialize with current UDS service configuration
            _config = _udsService.Configuration ?? new IsoTpConfig();

            // Initialize commands
            SaveConfigCommand = new RelayCommand(_ => SaveConfigAsync(), _ => IsModified);
            LoadConfigCommand = new RelayCommand(_ => LoadConfigAsync());
            ResetToDefaultsCommand = new RelayCommand(_ => ResetToDefaults());
            ApplyConfigCommand = new RelayCommand(_ => ApplyConfig(), _ => IsModified);

            IsModified = false;
        }

        private async void SaveConfigAsync()
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
                    Title = "Save ISO-TP Configuration"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    IsBusy = true;
                    StatusMessage = "Saving ISO-TP configuration...";

                    await _xmlService.SaveIsoTpConfigAsync(saveFileDialog.FileName, Config);
                    IsModified = false;

                    StatusMessage = "ISO-TP configuration saved successfully.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void LoadConfigAsync()
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
                    Title = "Open ISO-TP Configuration"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    IsBusy = true;
                    StatusMessage = "Loading ISO-TP configuration...";

                    var config = await _xmlService.LoadIsoTpConfigAsync(openFileDialog.FileName);
                    Config = config;

                    StatusMessage = "ISO-TP configuration loaded successfully.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ResetToDefaults()
        {
            Config = new IsoTpConfig();
            IsModified = true;
            StatusMessage = "Reset to default configuration.";
        }

        private void ApplyConfig()
        {
            _udsService.Configuration = Config;
            IsModified = false;
            StatusMessage = "Configuration applied to UDS service.";
        }
    }
}