using System.Xml.Serialization;

namespace UdsTool.Models
{
    [XmlRoot("IsoTpSettings")]
    public class IsoTpSettings
    {
        [XmlElement("RequestCanId")]
        public uint RequestCanId { get; set; }

        [XmlElement("ResponseCanId")]
        public uint ResponseCanId { get; set; }

        [XmlElement("FlowControlCanId")]
        public uint FlowControlCanId { get; set; }

        [XmlElement("BlockSize")]
        public byte BlockSize { get; set; }

        [XmlElement("SeparationTime")]
        public byte SeparationTime { get; set; }
    }
}