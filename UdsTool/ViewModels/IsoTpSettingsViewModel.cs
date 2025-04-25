using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;
using UdsTool.Models;

namespace UdsTool.ViewModels
{
    public class IsoTpSettingsViewModel : BaseViewModel
    {
        private IsoTpConfig _isoTpConfig = new IsoTpConfig();

        public IsoTpSettingsViewModel()
        {
            ApplySettingsCommand = new RelayCommand(_ => ApplySettings());
            ResetSettingsCommand = new RelayCommand(_ => ResetSettings());
            LoadSettingsCommand = new RelayCommand(_ => LoadSettings());
        }

        public IsoTpConfig IsoTpConfig => _isoTpConfig;

        public int BlockSize
        {
            get => _isoTpConfig.BlockSize;
            set
            {
                if (_isoTpConfig.BlockSize != value)
                {
                    _isoTpConfig.BlockSize = value;
                    OnPropertyChanged();
                }
            }
        }

        public int STmin
        {
            get => _isoTpConfig.STmin;
            set
            {
                if (_isoTpConfig.STmin != value)
                {
                    _isoTpConfig.STmin = value;
                    OnPropertyChanged();
                }
            }
        }

        public int MaxFrameSize
        {
            get => _isoTpConfig.MaxFrameSize;
            set
            {
                if (_isoTpConfig.MaxFrameSize != value)
                {
                    _isoTpConfig.MaxFrameSize = value;
                    OnPropertyChanged();
                }
            }
        }

        public int FirstFrameDataSize
        {
            get => _isoTpConfig.FirstFrameDataSize;
            set
            {
                if (_isoTpConfig.FirstFrameDataSize != value)
                {
                    _isoTpConfig.FirstFrameDataSize = value;
                    OnPropertyChanged();
                }
            }
        }

        public int ConsecutiveFrameDataSize
        {
            get => _isoTpConfig.ConsecutiveFrameDataSize;
            set
            {
                if (_isoTpConfig.ConsecutiveFrameDataSize != value)
                {
                    _isoTpConfig.ConsecutiveFrameDataSize = value;
                    OnPropertyChanged();
                }
            }
        }

        public int TimeoutMs
        {
            get => _isoTpConfig.TimeoutMs;
            set
            {
                if (_isoTpConfig.TimeoutMs != value)
                {
                    _isoTpConfig.TimeoutMs = value;
                    OnPropertyChanged();
                }
            }
        }

        public int FlowControlTimeoutMs
        {
            get => _isoTpConfig.FlowControlTimeoutMs;
            set
            {
                if (_isoTpConfig.FlowControlTimeoutMs != value)
                {
                    _isoTpConfig.FlowControlTimeoutMs = value;
                    OnPropertyChanged();
                }
            }
        }

        public uint RequestId
        {
            get => _isoTpConfig.RequestId;
            set
            {
                if (_isoTpConfig.RequestId != value)
                {
                    _isoTpConfig.RequestId = value;
                    OnPropertyChanged();
                }
            }
        }

        public uint ResponseId
        {
            get => _isoTpConfig.ResponseId;
            set
            {
                if (_isoTpConfig.ResponseId != value)
                {
                    _isoTpConfig.ResponseId = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand ApplySettingsCommand { get; }
        public ICommand ResetSettingsCommand { get; }
        public ICommand LoadSettingsCommand { get; }  // 추가된 명령

        // 설정 불러오기 메소드 추가
        private async void LoadSettings()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "ISO-TP 설정 파일 (*.isotp)|*.isotp|XML 파일 (*.xml)|*.xml|모든 파일 (*.*)|*.*",
                Title = "ISO-TP 설정 불러오기"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (FileStream stream = new FileStream(openFileDialog.FileName, FileMode.Open))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(IsoTpConfig));
                        var loadedConfig = (IsoTpConfig)serializer.Deserialize(stream);

                        // 불러온 설정 적용
                        _isoTpConfig = loadedConfig;

                        // 모든 속성 업데이트
                        OnPropertyChanged(nameof(BlockSize));
                        OnPropertyChanged(nameof(STmin));
                        OnPropertyChanged(nameof(MaxFrameSize));
                        OnPropertyChanged(nameof(FirstFrameDataSize));
                        OnPropertyChanged(nameof(ConsecutiveFrameDataSize));
                        OnPropertyChanged(nameof(TimeoutMs));
                        OnPropertyChanged(nameof(FlowControlTimeoutMs));
                        OnPropertyChanged(nameof(RequestId));
                        OnPropertyChanged(nameof(ResponseId));
                        OnPropertyChanged(nameof(IsoTpConfig));
                    }
                }
                catch (Exception ex)
                {
                    // 실제 구현에서는 오류 메시지를 표시하는 방법이 필요함
                    System.Diagnostics.Debug.WriteLine($"설정 불러오기 오류: {ex.Message}");
                }
            }
        }
        private void ApplySettings()
        {
            // 설정 적용 (속성 바인딩을 통해 자동으로 적용됨)
            OnPropertyChanged(nameof(IsoTpConfig));
        }

        private void ResetSettings()
        {
            // 기본 설정으로 초기화
            _isoTpConfig = new IsoTpConfig();

            // 모든 속성 업데이트
            OnPropertyChanged(nameof(BlockSize));
            OnPropertyChanged(nameof(STmin));
            OnPropertyChanged(nameof(MaxFrameSize));
            OnPropertyChanged(nameof(FirstFrameDataSize));
            OnPropertyChanged(nameof(ConsecutiveFrameDataSize));
            OnPropertyChanged(nameof(TimeoutMs));
            OnPropertyChanged(nameof(FlowControlTimeoutMs));
            OnPropertyChanged(nameof(RequestId));
            OnPropertyChanged(nameof(ResponseId));
            OnPropertyChanged(nameof(IsoTpConfig));
        }
    }
}
