using System;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Linq;

namespace UdsTool.Models
{
    [XmlRoot("DiagnosticFrame")]
    public class DiagnosticFrame
    {
        private byte _serviceId;
        private byte _subFunction;
        private ushort _dataIdentifier;
        private byte[] _data;

        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlIgnore]
        public byte ServiceId
        {
            get => _serviceId;
            set => _serviceId = value;
        }

        [XmlElement("SID")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string ServiceIdHex
        {
            get => _serviceId.ToString("X2");
            set => _serviceId = string.IsNullOrEmpty(value) ? (byte)0 : Convert.ToByte(value, 16);
        }

        [XmlIgnore]
        public byte SubFunction
        {
            get => _subFunction;
            set => _subFunction = value;
        }

        [XmlElement("SubFunction")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string SubFunctionHex
        {
            get => _subFunction.ToString("X2");
            set => _subFunction = string.IsNullOrEmpty(value) ? (byte)0 : Convert.ToByte(value, 16);
        }

        [XmlIgnore]
        public ushort DataIdentifier
        {
            get => _dataIdentifier;
            set => _dataIdentifier = value;
        }

        [XmlElement("DID")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string DataIdentifierHex
        {
            get => _dataIdentifier.ToString("X4");
            set => _dataIdentifier = string.IsNullOrEmpty(value) ? (ushort)0 : Convert.ToUInt16(value, 16);
        }

        [XmlIgnore]
        public byte[] Data
        {
            get => _data;
            set => _data = value;
        }

        [XmlElement("Data")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string DataHex
        {
            get
            {
                if (_data == null || _data.Length == 0)
                    return string.Empty;

                return BitConverter.ToString(_data).Replace("-", " ");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _data = new byte[0];
                }
                else
                {
                    var hexValues = value.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    _data = hexValues.Select(h => Convert.ToByte(h, 16)).ToArray();
                }
            }
        }

        [XmlElement("RequestResponseType")]
        public RequestResponseType Type { get; set; }

        [XmlIgnore]
        public ObservableCollection<DiagnosticFrame> Children { get; set; } = new ObservableCollection<DiagnosticFrame>();
    }

    public enum RequestResponseType
    {
        Request,
        Response
    }
}