using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace UdsTool.Core.Models
{
    [XmlInclude(typeof(ServiceId))]
    [XmlInclude(typeof(Subfunction))]
    [XmlInclude(typeof(DataId))]
    public class UdsElement : INotifyPropertyChanged
    {
        private string _name;
        private string _description;
        private byte _value;

        [XmlAttribute]
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        [XmlAttribute]
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged();
                }
            }
        }

        [XmlAttribute]
        public byte Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged();
                }
            }
        }

        [XmlIgnore]
        public string HexValue => $"0x{Value:X2}";

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ServiceId : UdsElement
    {
        private ObservableCollection<Subfunction> _subfunctions;
        private ObservableCollection<DataId> _dataIds;

        public ServiceId()
        {
            Subfunctions = new ObservableCollection<Subfunction>();
            DataIds = new ObservableCollection<DataId>();
        }

        public ObservableCollection<Subfunction> Subfunctions
        {
            get => _subfunctions;
            set
            {
                if (_subfunctions != value)
                {
                    _subfunctions = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<DataId> DataIds
        {
            get => _dataIds;
            set
            {
                if (_dataIds != value)
                {
                    _dataIds = value;
                    OnPropertyChanged();
                }
            }
        }
    }

    public class Subfunction : UdsElement
    {
        private ObservableCollection<UdsData> _data;

        public Subfunction()
        {
            Data = new ObservableCollection<UdsData>();
        }

        public ObservableCollection<UdsData> Data
        {
            get => _data;
            set
            {
                if (_data != value)
                {
                    _data = value;
                    OnPropertyChanged();
                }
            }
        }
    }

    public class DataId : UdsElement
    {
        private ObservableCollection<UdsData> _data;

        public DataId()
        {
            Data = new ObservableCollection<UdsData>();
        }

        public ObservableCollection<UdsData> Data
        {
            get => _data;
            set
            {
                if (_data != value)
                {
                    _data = value;
                    OnPropertyChanged();
                }
            }
        }
    }

    public class UdsData : INotifyPropertyChanged
    {
        private string _name;
        private string _value;

        [XmlAttribute]
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        [XmlAttribute]
        public string Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}