using System;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace UdsTool.Models
{
    [XmlRoot("DiagnosticFrame")]
    public class DiagnosticFrame
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlElement("SID")]
        public byte ServiceId { get; set; }

        [XmlElement("SubFunction")]
        public byte SubFunction { get; set; }

        [XmlElement("DID")]
        public ushort DataIdentifier { get; set; }

        [XmlArray("Data")]
        [XmlArrayItem("Byte")]
        public byte[] Data { get; set; }

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