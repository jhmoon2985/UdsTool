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
using System.ComponentModel;
using Microsoft.Win32;
using System.IO;
using System.Xml.Serialization;

namespace UdsTool.ViewModels
{
    public class EcuCommunicationViewModel : BaseViewModel
    {
        private readonly IEcuCommunicationService _communicationService;
        private readonly IXmlService _xmlService;
        private readonly IsoTpSettingsViewModel _isoTpSettingsViewModel;

        private DiagnosticSession _currentSession;
        private UdsXmlConfig _currentConfig;
        private string _requestText;
        private string _statusMessage;
        private bool _isConnected;
        private string _portName = "COM1";  // 기본값
        private UdsSid _selectedSid;
        private UdsSubfunction _selectedSubfunction;
        private UdsDid _selectedDid;

        public EcuCommunicationViewModel(
            IEcuCommunicationService communicationService,
            IXmlService xmlService,
            IsoTpSettingsViewModel isoTpSettingsViewModel)
        {
            _communicationService = communicationService;
            _xmlService = xmlService;
            _isoTpSettingsViewModel = isoTpSettingsViewModel;

            // 이벤트 연결
            _communicationService.FrameReceived += OnFrameReceived;
            _isoTpSettingsViewModel.PropertyChanged += OnIsoTpSettingsChanged;

            // 명령어 초기화
            ConnectCommand = new RelayCommand(_ => ConnectAsync());
            DisconnectCommand = new RelayCommand(_ => DisconnectAsync(), _ => IsConnected);
            SendRequestCommand = new RelayCommand(_ => SendRequestAsync(), _ => IsConnected && !string.IsNullOrWhiteSpace(RequestText));
            LoadConfigCommand = new RelayCommand(_ => LoadConfigAsync());
            SaveIsoTpSettingsCommand = new RelayCommand(_ => SaveIsoTpSettings());
            ClearLogsCommand = new RelayCommand(_ => ClearLogs());

            // 기본 세션 생성
            _currentSession = new DiagnosticSession();

            // 기본 UDS 설정 로드
            _currentConfig = _xmlService.CreateDefaultConfiguration();
        }

        public ObservableCollection<IsoTpFrame> Frames => _currentSession?.Frames;

        public string RequestText
        {
            get => _requestText;
            set
            {
                if (SetProperty(ref _requestText, value))
                {
                    ((RelayCommand)SendRequestCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                if (SetProperty(ref _isConnected, value))
                {
                    ((RelayCommand)DisconnectCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)SendRequestCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string PortName
        {
            get => _portName;
            set => SetProperty(ref _portName, value);
        }

        public UdsSid SelectedSid
        {
            get => _selectedSid;
            set
            {
                if (SetProperty(ref _selectedSid, value))
                {
                    SelectedSubfunction = null;
                    SelectedDid = null;

                    if (value != null)
                    {
                        UpdateRequestText();
                    }
                }
            }
        }

        public UdsSubfunction SelectedSubfunction
        {
            get => _selectedSubfunction;
            set
            {
                if (SetProperty(ref _selectedSubfunction, value))
                {
                    SelectedDid = null;

                    if (value != null)
                    {
                        UpdateRequestText();
                    }
                }
            }
        }

        public UdsDid SelectedDid
        {
            get => _selectedDid;
            set
            {
                if (SetProperty(ref _selectedDid, value))
                {
                    if (value != null)
                    {
                        UpdateRequestText();
                    }
                }
            }
        }

        public ObservableCollection<UdsSid> Services => new ObservableCollection<UdsSid>(_currentConfig?.Services ?? new System.Collections.Generic.List<UdsSid>());

        public IsoTpSettingsViewModel IsoTpSettings => _isoTpSettingsViewModel;

        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand SendRequestCommand { get; }
        public ICommand LoadConfigCommand { get; }
        public ICommand SaveIsoTpSettingsCommand { get; }
        public ICommand ClearLogsCommand { get; }

        private void UpdateRequestText()
        {
            if (SelectedSid == null) return;

            var sb = new StringBuilder();

            // SID 추가
            sb.Append(ConvertIdToHex(SelectedSid.Id));

            // Subfunction 추가
            if (SelectedSubfunction != null)
            {
                sb.Append(" ");
                sb.Append(ConvertIdToHex(SelectedSubfunction.Id));

                // DID 추가
                if (SelectedDid != null)
                {
                    sb.Append(" ");
                    sb.Append(ConvertIdToHex(SelectedDid.Id));

                    // 데이터 항목 추가 (더미 데이터)
                    if (SelectedDid.DataItems.Count > 0)
                    {
                        int totalLength = SelectedDid.DataItems.Sum(d => d.Length);
                        for (int i = 0; i < totalLength; i++)
                        {
                            sb.Append(" 00");  // 기본값으로 0x00 사용
                        }
                    }
                }
            }

            RequestText = sb.ToString();
        }

        private string ConvertIdToHex(string id)
        {
            // 0x 형식의 16진수 문자열을 처리
            if (id.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return id.Substring(2);
            }
            return id;
        }

        private async void ConnectAsync()
        {
            try
            {
                _communicationService.CurrentConfig = _isoTpSettingsViewModel.IsoTpConfig;
                bool result = await _communicationService.ConnectAsync(_portName);

                if (result)
                {
                    IsConnected = true;
                    _currentSession = new DiagnosticSession
                    {
                        IsoTpConfig = _isoTpSettingsViewModel.IsoTpConfig
                    };
                    OnPropertyChanged(nameof(Frames));
                    StatusMessage = $"연결되었습니다: {_portName}";
                }
                else
                {
                    StatusMessage = $"연결 실패: {_portName}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"연결 오류: {ex.Message}";
            }
        }

        private async void DisconnectAsync()
        {
            try
            {
                await _communicationService.DisconnectAsync();
                IsConnected = false;
                StatusMessage = "연결이 종료되었습니다.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"연결 종료 오류: {ex.Message}";
            }
        }

        private async void SendRequestAsync()
        {
            try
            {
                // 요청 텍스트를 바이트 배열로 변환
                byte[] requestData = ParseHexString(RequestText);

                if (requestData.Length == 0)
                {
                    StatusMessage = "요청 데이터가 비어 있습니다.";
                    return;
                }

                // 요청 전송
                byte[] responseData = await _communicationService.SendRequestAsync(requestData);

                // 응답 처리는 이벤트 핸들러에서 수행
                StatusMessage = "요청이 성공적으로 전송되었습니다.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"요청 전송 오류: {ex.Message}";
            }
        }

        private byte[] ParseHexString(string hexString)
        {
            if (string.IsNullOrWhiteSpace(hexString))
                return new byte[0];

            // 공백으로 분리된 16진수 문자열 처리
            string[] hexValues = hexString.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            byte[] result = new byte[hexValues.Length];

            for (int i = 0; i < hexValues.Length; i++)
            {
                string hexValue = hexValues[i].Trim();
                if (hexValue.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    hexValue = hexValue.Substring(2);

                if (!byte.TryParse(hexValue, System.Globalization.NumberStyles.HexNumber, null, out byte value))
                {
                    throw new FormatException($"잘못된 16진수 형식: {hexValues[i]}");
                }

                result[i] = value;
            }

            return result;
        }

        private void OnFrameReceived(object sender, IsoTpFrame frame)
        {
            // UI 스레드에서 프레임 추가
            App.Current.Dispatcher.Invoke(() =>
            {
                _currentSession.Frames.Add(frame);
                StatusMessage = $"프레임 수신: {frame.Description}";
            });
        }

        private void OnIsoTpSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (IsConnected)
            {
                _communicationService.CurrentConfig = _isoTpSettingsViewModel.IsoTpConfig;
                StatusMessage = "ISO-TP 설정이 업데이트되었습니다.";
            }
        }

        private async void LoadConfigAsync()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "XML 파일 (*.xml)|*.xml|모든 파일 (*.*)|*.*",
                Title = "UDS 설정 파일 열기"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _currentConfig = await _xmlService.LoadConfigurationAsync(openFileDialog.FileName);
                    OnPropertyChanged(nameof(Services));
                    StatusMessage = $"설정 파일을 성공적으로 불러왔습니다: {openFileDialog.FileName}";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"설정 파일을 불러오는 중 오류가 발생했습니다: {ex.Message}";
                }
            }
        }

        // SaveIsoTpSettings 메소드 구현
        private async void SaveIsoTpSettings()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "ISO-TP 설정 파일 (*.isotp)|*.isotp|XML 파일 (*.xml)|*.xml|모든 파일 (*.*)|*.*",
                Title = "ISO-TP 설정 저장",
                FileName = $"IsoTpConfig_{DateTime.Now:yyyyMMdd}.isotp"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    await SaveIsoTpConfigToFileAsync(_isoTpSettingsViewModel.IsoTpConfig, saveFileDialog.FileName);
                    StatusMessage = $"ISO-TP 설정이 성공적으로 저장되었습니다: {saveFileDialog.FileName}";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"ISO-TP 설정 저장 중 오류가 발생했습니다: {ex.Message}";
                }
            }
        }

        // ISO-TP 설정을 파일로 저장하는 메소드
        private async Task SaveIsoTpConfigToFileAsync(IsoTpConfig config, string filePath)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(IsoTpConfig));
                serializer.Serialize(stream, config);
            }
        }

        private void ClearLogs()
        {
            _currentSession.Frames.Clear();
            StatusMessage = "로그가 초기화되었습니다.";
        }
    }
}
