using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using UdsTool.Commands;
using UdsTool.Models;
using UdsTool.Services;
using Microsoft.Win32;

namespace UdsTool.ViewModels
{
    public class EcuCommunicationViewModel : INotifyPropertyChanged
    {
        private readonly IEcuCommunicationService _ecuService;
        private readonly IXmlService _xmlService;
        private DiagnosticFrame _selectedFrame;
        private IsoTpSettings _isoTpSettings;
        private string _communicationLog;
        private bool _isConnected;

        public EcuCommunicationViewModel(IEcuCommunicationService ecuService, IXmlService xmlService)
        {
            _ecuService = ecuService;
            _xmlService = xmlService;

            // 초기화는 한 번만 수행
            if (DiagnosticFrames == null)
            {
                DiagnosticFrames = new ObservableCollection<DiagnosticFrame>();

                // 기본 IsoTpSettings 설정
                IsoTpSettings = new IsoTpSettings
                {
                    RequestCanId = 0x7E0,
                    ResponseCanId = 0x7E8,
                    FlowControlCanId = 0x7E0,
                    BlockSize = 0,
                    SeparationTime = 0
                };
            }

            ConnectCommand = new RelayCommand(_ => Connect(), _ => !IsConnected);
            DisconnectCommand = new RelayCommand(_ => Disconnect(), _ => IsConnected);
            SendRequestCommand = new RelayCommand(_ => SendRequest(), _ => IsConnected && SelectedFrame != null);
            SaveSettingsCommand = new RelayCommand(_ => SaveSettings());
            LoadSettingsCommand = new RelayCommand(_ => LoadSettings());
            LoadFramesCommand = new RelayCommand(_ => LoadFrames());
            ClearLogCommand = new RelayCommand(_ => ClearLog());

            // 이벤트 핸들러 등록 (중복 등록 방지)
            _ecuService.DataReceived -= OnDataReceived; // 기존 핸들러 제거
            _ecuService.DataReceived += OnDataReceived; // 새 핸들러 등록
        }

        public ObservableCollection<DiagnosticFrame> DiagnosticFrames { get; }

        public DiagnosticFrame SelectedFrame
        {
            get => _selectedFrame;
            set
            {
                _selectedFrame = value;
                OnPropertyChanged();
            }
        }

        public IsoTpSettings IsoTpSettings
        {
            get => _isoTpSettings;
            set
            {
                _isoTpSettings = value;
                OnPropertyChanged();
            }
        }

        public string CommunicationLog
        {
            get => _communicationLog;
            set
            {
                _communicationLog = value;
                OnPropertyChanged();
            }
        }

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnPropertyChanged();
            }
        }

        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand SendRequestCommand { get; }
        public ICommand SaveSettingsCommand { get; }
        public ICommand LoadSettingsCommand { get; }
        public ICommand LoadFramesCommand { get; }
        public ICommand ClearLogCommand { get; }

        private void Connect()
        {
            try
            {
                _ecuService.UpdateIsoTpSettings(IsoTpSettings);
                _ecuService.Connect();
                IsConnected = true;
                AddLog("Connected to ECU");
            }
            catch (Exception ex)
            {
                AddLog($"Error: {ex.Message}");
            }
        }

        private void Disconnect()
        {
            try
            {
                _ecuService.Disconnect();
                IsConnected = false;
                AddLog("Disconnected from ECU");
            }
            catch (Exception ex)
            {
                AddLog($"Error: {ex.Message}");
            }
        }

        private void SendRequest()
        {
            try
            {
                if (SelectedFrame != null)
                {
                    _ecuService.SendRequest(SelectedFrame);
                    AddLog($"Sent request: {SelectedFrame.Name}");
                }
            }
            catch (Exception ex)
            {
                AddLog($"Error: {ex.Message}");
            }
        }

        private void SaveSettings()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                DefaultExt = "xml",
                FileName = "IsoTpSettings.xml"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var serializer = new System.Xml.Serialization.XmlSerializer(typeof(IsoTpSettings));
                    using (var writer = new System.IO.StreamWriter(saveFileDialog.FileName))
                    {
                        serializer.Serialize(writer, IsoTpSettings);
                    }
                    AddLog("Settings saved successfully");
                }
                catch (Exception ex)
                {
                    AddLog($"Error saving settings: {ex.Message}");
                }
            }
        }

        private void LoadSettings()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                DefaultExt = "xml"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var serializer = new System.Xml.Serialization.XmlSerializer(typeof(IsoTpSettings));
                    using (var reader = new System.IO.StreamReader(openFileDialog.FileName))
                    {
                        IsoTpSettings = (IsoTpSettings)serializer.Deserialize(reader);
                    }
                    AddLog("Settings loaded successfully");
                }
                catch (Exception ex)
                {
                    AddLog($"Error loading settings: {ex.Message}");
                }
            }
        }

        private void LoadFrames()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                DefaultExt = "xml"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var loadedFrames = _xmlService.LoadFromFile(openFileDialog.FileName);
                    DiagnosticFrames.Clear();
                    foreach (var frame in loadedFrames)
                    {
                        DiagnosticFrames.Add(frame);
                    }
                    AddLog("Frames loaded successfully");
                }
                catch (Exception ex)
                {
                    AddLog($"Error loading frames: {ex.Message}");
                }
            }
        }

        private void ClearLog()
        {
            CommunicationLog = string.Empty;
        }

        private void OnDataReceived(object sender, string data)
        {
            AddLog(data);
        }

        private void AddLog(string message)
        {
            CommunicationLog += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}