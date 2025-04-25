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
    public class EcuCommunicationViewModel : ViewModelBase
    {
        private readonly IEcuCommunicationService _ecuService;
        private readonly IXmlService _xmlService;
        private UdsConfiguration _configuration;
        private UdsCommand _selectedCommand;
        private UdsResponse _lastResponse;
        private string _statusMessage;
        private bool _isConnected;
        private CancellationTokenSource _cts;

        public UdsConfiguration Configuration
        {
            get => _configuration;
            set
            {
                if (SetProperty(ref _configuration, value))
                {
                    OnPropertyChanged(nameof(Commands));
                }
            }
        }

        public ObservableCollection<UdsCommand> Commands =>
            _configuration != null ? new ObservableCollection<UdsCommand>(_configuration.Commands) : new ObservableCollection<UdsCommand>();

        public ObservableCollection<UdsResponse> ResponseHistory { get; } = new ObservableCollection<UdsResponse>();

        public UdsCommand SelectedCommand
        {
            get => _selectedCommand;
            set => SetProperty(ref _selectedCommand, value);
        }

        public UdsResponse LastResponse
        {
            get => _lastResponse;
            set => SetProperty(ref _lastResponse, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }

        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand SendCommand { get; }
        public ICommand LoadConfigurationCommand { get; }
        public ICommand ClearHistoryCommand { get; }

        public EcuCommunicationViewModel(IEcuCommunicationService ecuService, IXmlService xmlService)
        {
            _ecuService = ecuService ?? throw new ArgumentNullException(nameof(ecuService));
            _xmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));

            // Initialize with empty configuration
            Configuration = new UdsConfiguration();

            // Subscribe to response events
            _ecuService.ResponseReceived += OnResponseReceived;

            // Commands
            ConnectCommand = new RelayCommand(_ => ConnectAsync(), _ => !IsConnected);
            DisconnectCommand = new RelayCommand(_ => DisconnectAsync(), _ => IsConnected);
            SendCommand = new RelayCommand(_ => SendSelectedCommandAsync(), _ => IsConnected && SelectedCommand != null);
            LoadConfigurationCommand = new RelayCommand(_ => LoadConfigurationAsync());
            ClearHistoryCommand = new RelayCommand(_ => ResponseHistory.Clear());
        }

        private async void ConnectAsync()
        {
            try
            {
                StatusMessage = "Connecting...";
                IsConnected = await _ecuService.ConnectAsync(Configuration.ConnectionSettings);
                StatusMessage = IsConnected ? "Connected" : "Connection failed";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Connection error: {ex.Message}";
                IsConnected = false;
            }
        }

        private async void DisconnectAsync()
        {
            try
            {
                _cts?.Cancel();
                StatusMessage = "Disconnecting...";
                bool success = await _ecuService.DisconnectAsync();
                IsConnected = !success;
                StatusMessage = success ? "Disconnected" : "Disconnection failed";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Disconnection error: {ex.Message}";
            }
        }

        private async void SendSelectedCommandAsync()
        {
            if (SelectedCommand == null || !IsConnected)
                return;

            try
            {
                _cts?.Cancel();
                _cts = new CancellationTokenSource();

                StatusMessage = $"Sending command: {SelectedCommand.Name}";

                LastResponse = await _ecuService.SendCommandAsync(SelectedCommand, _cts.Token);

                StatusMessage = $"Response received: {LastResponse}";
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Command canceled";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error sending command: {ex.Message}";
            }
        }

        private async void LoadConfigurationAsync()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
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
                    StatusMessage = $"Configuration loaded from {openFileDialog.FileName}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading configuration: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OnResponseReceived(object sender, UdsResponse response)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ResponseHistory.Insert(0, response);
                if (ResponseHistory.Count > 100)
                {
                    ResponseHistory.RemoveAt(ResponseHistory.Count - 1);
                }
            });
        }
    }
}
