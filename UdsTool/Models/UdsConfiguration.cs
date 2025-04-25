using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace UdsTool.Models
{
    [Serializable]
    [XmlRoot("UdsConfiguration")]
    public class UdsConfiguration
    {
        [XmlArray("Commands")]
        [XmlArrayItem("Command")]
        public List<UdsCommand> Commands { get; set; } = new List<UdsCommand>();

        [XmlElement("ConnectionSettings")]
        public ConnectionSettings ConnectionSettings { get; set; } = new ConnectionSettings();
    }

    [Serializable]
    public class ConnectionSettings
    {
        [XmlElement("InterfaceType")]
        public string InterfaceType { get; set; } = "CAN";

        [XmlElement("DeviceId")]
        public string DeviceId { get; set; } = "com1";

        [XmlElement("Baudrate")]
        public int Baudrate { get; set; } = 500000;

        [XmlElement("SourceAddress")]
        public byte SourceAddress { get; set; } = 0xF1;

        [XmlElement("TargetAddress")]
        public byte TargetAddress { get; set; } = 0x01;

        [XmlElement("Timeout")]
        public int TimeoutMs { get; set; } = 1000;
    }
}
