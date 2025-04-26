using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace UdsTool.Core.Models
{
    [Serializable]
    public class IsoTpConfig : INotifyPropertyChanged
    {
        private uint _requestCanId;
        private uint _responseCanId;
        private byte _blockSize;
        private byte _separationTime;
        private int _timeout;
        private bool _paddingEnabled;
        private bool _extendedAddressing;
        private bool _useCanFd;

        [XmlAttribute]
        public uint RequestCanId
        {
            get => _requestCanId;
            set
            {
                if (_requestCanId != value)
                {
                    _requestCanId = value;
                    OnPropertyChanged();
                }
            }
        }

        [XmlAttribute]
        public uint ResponseCanId
        {
            get => _responseCanId;
            set
            {
                if (_responseCanId != value)
                {
                    _responseCanId = value;
                    OnPropertyChanged();
                }
            }
        }

        [XmlAttribute]
        public byte BlockSize
        {
            get => _blockSize;
            set
            {
                if (_blockSize != value)
                {
                    _blockSize = value;
                    OnPropertyChanged();
                }
            }
        }

        [XmlAttribute]
        public byte SeparationTime
        {
            get => _separationTime;
            set
            {
                if (_separationTime != value)
                {
                    _separationTime = value;
                    OnPropertyChanged();
                }
            }
        }

        [XmlAttribute]
        public int Timeout
        {
            get => _timeout;
            set
            {
                if (_timeout != value)
                {
                    _timeout = value;
                    OnPropertyChanged();
                }
            }
        }

        [XmlAttribute]
        public bool PaddingEnabled
        {
            get => _paddingEnabled;
            set
            {
                if (_paddingEnabled != value)
                {
                    _paddingEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        [XmlAttribute]
        public bool ExtendedAddressing
        {
            get => _extendedAddressing;
            set
            {
                if (_extendedAddressing != value)
                {
                    _extendedAddressing = value;
                    OnPropertyChanged();
                }
            }
        }

        [XmlAttribute]
        public bool UseCanFd
        {
            get => _useCanFd;
            set
            {
                if (_useCanFd != value)
                {
                    _useCanFd = value;
                    OnPropertyChanged();
                }
            }
        }

        public IsoTpConfig()
        {
            // Default values
            RequestCanId = 0x7E0;
            ResponseCanId = 0x7E8;
            BlockSize = 8;
            SeparationTime = 20;
            Timeout = 1000;
            PaddingEnabled = true;
            ExtendedAddressing = false;
            UseCanFd = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [Serializable]
    public class UdsDatabase
    {
        public UdsDatabase()
        {
            ServiceIds = new System.Collections.ObjectModel.ObservableCollection<ServiceId>();
            IsoTpConfig = new IsoTpConfig();
        }

        public System.Collections.ObjectModel.ObservableCollection<ServiceId> ServiceIds { get; set; }
        public IsoTpConfig IsoTpConfig { get; set; }
    }
}