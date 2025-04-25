using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace UdsTool.Models
{
    [XmlRoot("UdsConfiguration")]
    public class UdsXmlConfig
    {
        [XmlArray("Services")]
        [XmlArrayItem("Sid")]
        public List<UdsSid> Services { get; set; } = new List<UdsSid>();

        public string FilePath { get; set; }
    }
}
