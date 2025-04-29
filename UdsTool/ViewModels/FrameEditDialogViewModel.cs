using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using UdsTool.Commands;
using UdsTool.Models;
using UdsTool.Services;

namespace UdsTool.ViewModels
{
    public class FrameEditDialogViewModel : INotifyPropertyChanged
    {
        private string _name;
        private byte _selectedServiceId;
        private byte _selectedSubFunction;
        private ushort _selectedDid;
        private string _serviceIdHex;
        private string _subFunctionHex;
        private string _didHex;
        private string _data;
        private int _idx;
        private int _selectedRequestIdx;
        private string _responseIdxText;
        private RequestResponseType _type;
        private ICloseable _window;
        private Dictionary<byte, string> _subFunctions;
        private Dictionary<int, string> _availableRequests;

        public FrameEditDialogViewModel()
        {
            InitializeValues();

            OkCommand = new RelayCommand(_ => Ok(), _ => ValidateInput());
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        public FrameEditDialogViewModel(DiagnosticFrame frame) : this()
        {
            if (frame != null)
            {
                Idx = frame.Idx;
                Name = frame.Name;
                SelectedServiceId = frame.ServiceId;
                SelectedSubFunction = frame.SubFunction;
                SelectedDid = frame.DataIdentifier;
                Type = frame.Type;

                // Response의 경우 ResponseIdx 설정
                if (frame.Type == RequestResponseType.Response)
                {
                    SelectedRequestIdx = frame.ResponseIdx;
                    ResponseIdxText = frame.ResponseIdx.ToString();
                }

                if (frame.Data != null && frame.Data.Length > 0)
                {
                    Data = BitConverter.ToString(frame.Data).Replace("-", " ");
                }
            }
        }

        private void InitializeValues()
        {
            // 타입별 기본값 설정
            if (Type == RequestResponseType.Request)
            {
                Name = "New Request";
            }
            else
            {
                Name = "New Response";
            }

            SelectedServiceId = 0x10;
            SelectedSubFunction = 0x01;
            SelectedDid = 0xF186;
            Data = "";
            SelectedRequestIdx = 0;
            ResponseIdxText = "0";
            AvailableRequests = new Dictionary<int, string>();
        }

        public void SetWindow(ICloseable window)
        {
            _window = window;
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public int Idx
        {
            get => _idx;
            set { _idx = value; }
        }

        // Request 콤보박스용 속성
        public Dictionary<int, string> AvailableRequests
        {
            get => _availableRequests;
            set
            {
                _availableRequests = value;
                OnPropertyChanged();
            }
        }

        // 콤보박스에서 선택된 Request의 Idx
        public int SelectedRequestIdx
        {
            get => _selectedRequestIdx;
            set
            {
                _selectedRequestIdx = value;
                ResponseIdxText = value.ToString();
                OnPropertyChanged();
            }
        }

        // 직접 입력용 텍스트 필드
        public string ResponseIdxText
        {
            get => _responseIdxText;
            set
            {
                _responseIdxText = value;
                if (int.TryParse(value, out int result))
                {
                    _selectedRequestIdx = result;
                }
                OnPropertyChanged();
            }
        }

        public Dictionary<byte, string> ServiceIdentifiers => UdsDefinitions.ServiceIdentifiers;

        public byte SelectedServiceId
        {
            get => _selectedServiceId;
            set
            {
                _selectedServiceId = value;
                ServiceIdHex = value.ToString("X2");
                UpdateSubFunctions();
                OnPropertyChanged();
            }
        }

        public string ServiceIdHex
        {
            get => _serviceIdHex;
            set
            {
                _serviceIdHex = value;
                if (byte.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out byte result))
                {
                    _selectedServiceId = result;
                    UpdateSubFunctions();
                }
                OnPropertyChanged();
            }
        }

        public Dictionary<byte, string> SubFunctions
        {
            get => _subFunctions;
            set { _subFunctions = value; OnPropertyChanged(); }
        }

        public byte SelectedSubFunction
        {
            get => _selectedSubFunction;
            set
            {
                _selectedSubFunction = value;
                SubFunctionHex = value.ToString("X2");
                OnPropertyChanged();
            }
        }

        public string SubFunctionHex
        {
            get => _subFunctionHex;
            set
            {
                _subFunctionHex = value;
                if (byte.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out byte result))
                {
                    _selectedSubFunction = result;
                }
                OnPropertyChanged();
            }
        }

        public Dictionary<ushort, string> DataIdentifiers => UdsDefinitions.DataIdentifiers;

        public ushort SelectedDid
        {
            get => _selectedDid;
            set
            {
                _selectedDid = value;
                DidHex = value.ToString("X4");
                OnPropertyChanged();
            }
        }

        public string DidHex
        {
            get => _didHex;
            set
            {
                _didHex = value;
                if (ushort.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out ushort result))
                {
                    _selectedDid = result;
                }
                OnPropertyChanged();
            }
        }

        public string Data
        {
            get => _data;
            set { _data = value; OnPropertyChanged(); }
        }

        public RequestResponseType Type
        {
            get => _type;
            set
            {
                _type = value;

                // 유형이 변경될 때 이름 자동 업데이트
                if (_name == "New Request" || _name == "New Response" || string.IsNullOrEmpty(_name))
                {
                    Name = value == RequestResponseType.Request ? "New Request" : "New Response";
                }

                OnPropertyChanged();
                // Response 타입이 사용하는 속성들의 가시성 업데이트를 위해 알림
                OnPropertyChanged(nameof(IsResponseType));
            }
        }

        // ResponseIdx 관련 컨트롤의 가시성 제어를 위한 속성
        public bool IsResponseType => Type == RequestResponseType.Response;

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        private void UpdateSubFunctions()
        {
            if (UdsDefinitions.SubFunctions.TryGetValue(_selectedServiceId, out var subFunctions))
            {
                SubFunctions = subFunctions;
                if (subFunctions.Count > 0)
                {
                    SelectedSubFunction = subFunctions.Keys.First();
                }
            }
            else
            {
                SubFunctions = new Dictionary<byte, string>();
                SelectedSubFunction = 0;
            }
        }

        public DiagnosticFrame GetFrame()
        {
            return new DiagnosticFrame
            {
                Idx = Idx,
                Name = Name,
                ServiceId = _selectedServiceId,
                SubFunction = _selectedSubFunction,
                DataIdentifier = _selectedDid,
                Type = Type,
                ResponseIdx = Type == RequestResponseType.Response ? _selectedRequestIdx : 0,
                Data = ParseData(),
                Children = new ObservableCollection<DiagnosticFrame>() // 컬렉션 초기화 확인
            };
        }

        private byte[] ParseData()
        {
            if (string.IsNullOrWhiteSpace(Data))
                return new byte[0];

            var hexValues = Data.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            return hexValues.Select(h => Convert.ToByte(h, 16)).ToArray();
        }

        private bool ValidateInput()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Name))
                    return false;

                if (!string.IsNullOrWhiteSpace(Data))
                {
                    ParseData();
                }

                // Response인 경우 ResponseIdx 유효성 검사
                if (Type == RequestResponseType.Response)
                {
                    // ResponseIdxText가 유효한 정수인지 확인
                    if (!int.TryParse(ResponseIdxText, out _))
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void Ok()
        {
            _window?.Close(true);
        }

        private void Cancel()
        {
            _window?.Close(false);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}