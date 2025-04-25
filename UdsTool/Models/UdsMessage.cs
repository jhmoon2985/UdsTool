using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace UdsTool.Models
{
    public enum UdsMessageType
    {
        Sid,
        Subfunction,
        Did,
        Data
    }

    // UDS 메시지 기본 클래스
    public class UdsMessage
    {
        [XmlAttribute("Id")]
        public string Id { get; set; }

        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Description")]
        public string Description { get; set; }

        [XmlElement("Value")]
        public string Value { get; set; }
    }

    // SID (Service Identifier) 정의
    public class UdsSid : UdsMessage
    {
        [XmlArray("Subfunctions")]
        [XmlArrayItem("Subfunction")]
        public List<UdsSubfunction> Subfunctions { get; set; } = new List<UdsSubfunction>();
    }

    // Subfunction 정의
    public class UdsSubfunction : UdsMessage
    {
        [XmlArray("Dids")]
        [XmlArrayItem("Did")]
        public List<UdsDid> Dids { get; set; } = new List<UdsDid>();
    }

    // DID (Data Identifier) 정의
    public class UdsDid : UdsMessage
    {
        [XmlArray("DataItems")]
        [XmlArrayItem("DataItem")]
        public List<UdsDataItem> DataItems { get; set; } = new List<UdsDataItem>();
    }

    // Data Item 정의
    public class UdsDataItem : UdsMessage
    {
        [XmlAttribute("Length")]
        public int Length { get; set; }

        [XmlAttribute("Unit")]
        public string Unit { get; set; }
    }
}
