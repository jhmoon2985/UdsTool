using System;
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
        private string _serviceId;
        private string _subFunction;
        private string _did;
        private string _data;
        private RequestResponseType _type;
        private bool? _dialogResult;
        private ICloseable _window;

        public FrameEditDialogViewModel(ICloseable window = null)
        {
            _window = window;
            // 기본값 설정
            Name = "New Frame";
            ServiceId = "10";
            SubFunction = "01";
            Did = "0000";
            Data = "";
            Type = RequestResponseType.Request;

            OkCommand = new RelayCommand(_ => Ok(), _ => ValidateInput());
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        public FrameEditDialogViewModel(DiagnosticFrame frame, ICloseable window = null) : this(window)
        {
            if (frame != null)
            {
                Name = frame.Name;
                ServiceId = frame.ServiceId.ToString("X2");
                SubFunction = frame.SubFunction.ToString("X2");
                Did = frame.DataIdentifier.ToString("X4");
                Type = frame.Type;

                if (frame.Data != null && frame.Data.Length > 0)
                {
                    Data = BitConverter.ToString(frame.Data).Replace("-", " ");
                }
            }
        }
        public void SetWindow(ICloseable window)
        {
            _window = window;
        }
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string ServiceId
        {
            get => _serviceId;
            set
            {
                _serviceId = value;
                OnPropertyChanged();
            }
        }

        public string SubFunction
        {
            get => _subFunction;
            set
            {
                _subFunction = value;
                OnPropertyChanged();
            }
        }

        public string Did
        {
            get => _did;
            set
            {
                _did = value;
                OnPropertyChanged();
            }
        }

        public string Data
        {
            get => _data;
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        public RequestResponseType Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged();
            }
        }

        public bool? DialogResult
        {
            get => _dialogResult;
            set
            {
                _dialogResult = value;
                OnPropertyChanged();
            }
        }

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        public DiagnosticFrame GetFrame()
        {
            return new DiagnosticFrame
            {
                Name = Name,
                ServiceId = Convert.ToByte(ServiceId, 16),
                SubFunction = Convert.ToByte(SubFunction, 16),
                DataIdentifier = Convert.ToUInt16(Did, 16),
                Type = Type,
                Data = ParseData()
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

                Convert.ToByte(ServiceId, 16);
                Convert.ToByte(SubFunction, 16);
                Convert.ToUInt16(Did, 16);

                if (!string.IsNullOrWhiteSpace(Data))
                {
                    ParseData();
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
            var s = ServiceId;
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