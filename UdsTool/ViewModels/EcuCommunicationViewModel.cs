using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UdsTool.Core.Base;
using UdsTool.Core.Interfaces;
using UdsTool.Core.Models;

namespace UdsTool.ViewModels
{
    public class EcuCommunicationViewModel : ViewModelBase
    {
        private readonly IUdsService _udsService;
        private readonly ICanService _canService;
        private readonly IXmlService _xmlService;
        private readonly INavigationService _navigationService;

        private string _selectedInterface;
        private bool _isConnected;
        private string _requestHex;
        private string _responseHex;
        private ObservableCollection<string> _interfaces;
        private ObservableCollection<string> _messageLog;
        private IsoTpConfig _isoTpConfig;
        private bool _showIsoTpConfig;

        public string SelectedInterface
        {
            get => _selectedInterface;
            set => SetProperty(ref _selectedInterface, value);
        }

        public bool IsConnected
        {
            get => _isConnected;
            private set => SetProperty(ref _isConnected, value);
        }

        public string RequestHex
        {
            get => _requestHex;
            set => SetProperty(ref _requestHex, value);
        }

        public string ResponseHex
        {
            get => _responseHex;
            set => SetProperty(ref _responseHex, value);
        }

        public ObservableCollection<string> Interfaces
        {
            get => _interfaces;
            private set => SetProperty(ref _interfaces, value);
        }

        public ObservableCollection<string> MessageLog
        {
            get => _messageLog;
            private set => SetProperty(ref _messageLog, value);
        }

        public IsoTpConfig IsoTpConfig
        {
            get => _isoTpConfig;
            set
            {
                SetProperty(ref _isoTpConfig, value);
                _udsService.Configuration = value;
            }
        }

        public bool ShowIsoTpConfig
        {
            get => _showIsoTpConfig;
            set => SetProperty(ref _showIsoTpConfig, value);
        }

        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand SendRequestCommand { get; }
        public ICommand ClearLogCommand { get; }
        public ICommand ConfigureIsoTpCommand { get; }
        public ICommand SaveIsoTpConfigCommand { get; }
        public ICommand LoadIsoTpConfigCommand { get; }
        public ICommand NavigateToIsoTpConfigCommand { get; }

        public EcuCommunicationViewModel(
            IUdsService udsService,
            ICanService canService,
            IXmlService xmlService,
            INavigationService navigationService)
        {
            _udsService = udsService ?? throw new ArgumentNullException(nameof(udsService));
            _canService = canService ?? throw new ArgumentNullException(nameof(canService));
            _xmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            // Initialize properties
            Interfaces = new ObservableCollection<string>(GetAvailableInterfaces());
            MessageLog = new ObservableCollection<string>();
            IsoTpConfig = new IsoTpConfig();
            RequestHex = "";
            ResponseHex = "";

            // Initialize commands
            ConnectCommand = new RelayCommand(_ => ConnectAsync(), _ => !IsConnected && !string.IsNullOrEmpty(SelectedInterface));
            DisconnectCommand = new RelayCommand(_ => DisconnectAsync(), _ => IsConnected);
            SendRequestCommand = new RelayCommand(_ => SendRequestAsync(), _ => IsConnected && !string.IsNullOrEmpty(RequestHex));
            ClearLogCommand = new RelayCommand(_ => MessageLog.Clear());
            ConfigureIsoTpCommand = new RelayCommand(_ => ShowIsoTpConfig = !ShowIsoTpConfig);
            SaveIsoTpConfigCommand = new RelayCommand(_ => SaveIsoTpConfigAsync());
            LoadIsoTpConfigCommand = new RelayCommand(_ => LoadIsoTpConfigAsync());
            NavigateToIsoTpConfigCommand = new RelayCommand(_ => _navigationService.NavigateTo<IsoTpConfigViewModel>());

            // Subscribe to UDS service events
            _udsService.ResponseReceived += OnResponseReceived;
            _udsService.MessageLogged += OnMessageLogged;
        }

        private List<string> GetAvailableInterfaces()
        {
            // In a real implementation, this would enumerate all available CAN interfaces
            // For this example, return some mock interfaces
            return new List<string> { "vcan0", "can0", "can1", "slcan0", "peak_usb0", "vector0" };
        }

        private async void ConnectAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Connecting to CAN interface...";

                var connected = await _canService.ConnectAsync(SelectedInterface);
                if (connected)
                {
                    await _udsService.InitializeAsync(_canService);
                    IsConnected = true;
                    AddLogMessage($"Connected to {SelectedInterface}");
                    StatusMessage = "Connected to CAN interface.";
                }
                else
                {
                    StatusMessage = "Failed to connect to CAN interface.";
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

        private async void DisconnectAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Disconnecting from CAN interface...";

                _udsService.Shutdown();
                await _canService.DisconnectAsync();
                IsConnected = false;

                AddLogMessage($"Disconnected from {SelectedInterface}");
                StatusMessage = "Disconnected from CAN interface.";
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

        private async void SendRequestAsync()
        {
            try
            {
                // Parse hex string to byte array
                byte[] request = ParseHexString(RequestHex);
                if (request == null || request.Length == 0)
                {
                    StatusMessage = "Invalid hex format";
                    return;
                }

                IsBusy = true;
                StatusMessage = "Sending UDS request...";

                AddLogMessage($"TX: {BitConverter.ToString(request)}");

                var response = await _udsService.SendRequestAsync(request);
                if (response != null)
                {
                    ResponseHex = BitConverter.ToString(response).Replace("-", " ");
                    AddLogMessage($"RX: {BitConverter.ToString(response)}");
                    StatusMessage = "Request completed.";
                }
                else
                {
                    StatusMessage = "No response received.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                AddLogMessage($"Error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void OnResponseReceived(object sender, byte[] response)
        {
            // Response is already handled in SendRequestAsync
            // This event handler is for unsolicited responses
            if (response != null)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    ResponseHex = BitConverter.ToString(response).Replace("-", " ");
                    AddLogMessage($"RX (Unsolicited): {BitConverter.ToString(response)}");
                });
            }
        }

        private void OnMessageLogged(object sender, string message)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                AddLogMessage(message);
            });
        }

        private void AddLogMessage(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            MessageLog.Add($"[{timestamp}] {message}");

            // Limit log size
            while (MessageLog.Count > 1000)
            {
                MessageLog.RemoveAt(0);
            }
        }

        private byte[] ParseHexString(string hexString)
        {
            try
            {
                // Remove spaces, dashes, etc.
                hexString = hexString.Replace(" ", "").Replace("-", "");

                // Check if string has a valid length
                if (hexString.Length % 2 != 0)
                    return null;

                // Convert to byte array
                var byteArray = new byte[hexString.Length / 2];
                for (int i = 0; i < byteArray.Length; i++)
                {
                    byteArray[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
                }

                return byteArray;
            }
            catch
            {
                return null;
            }
        }

        private async void SaveIsoTpConfigAsync()
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

                    await _xmlService.SaveIsoTpConfigAsync(saveFileDialog.FileName, IsoTpConfig);

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

        private async void LoadIsoTpConfigAsync()
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
                    IsoTpConfig = config;

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
    }