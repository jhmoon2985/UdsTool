using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace UdsTool.Models
{
    [Serializable]
    public class UdsCommand
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlElement("ServiceId")]
        public byte ServiceId { get; set; }

        [XmlElement("Subfunction")]
        public byte? Subfunction { get; set; }

        [XmlElement("Did")]
        public ushort? Did { get; set; }

        [XmlArray("Data")]
        [XmlArrayItem("Byte")]
        public List<byte> Data { get; set; } = new List<byte>();

        public override string ToString()
        {
            return $"{Name} - SID: 0x{ServiceId:X2}" +
                   (Subfunction.HasValue ? $", SF: 0x{Subfunction:X2}" : "") +
                   (Did.HasValue ? $", DID: 0x{Did:X4}" : "");
        }

        public byte[] ToByteArray()
        {
            var result = new List<byte>();
            result.Add(ServiceId);

            if (Subfunction.HasValue)
                result.Add(Subfunction.Value);

            if (Did.HasValue)
            {
                result.Add((byte)((Did.Value >> 8) & 0xFF));
                result.Add((byte)(Did.Value & 0xFF));
            }

            if (Data != null && Data.Count > 0)
                result.AddRange(Data);

            return result.ToArray();
        }
    }
}
