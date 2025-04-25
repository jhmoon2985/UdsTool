using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UdsTool.Models
{
    [Serializable]
    public class UdsResponse
    {
        public byte ServiceId { get; set; }
        public List<byte> RawData { get; set; } = new List<byte>();
        public bool IsPositive => (ServiceId & 0x40) == 0x40;
        public byte NegativeResponseCode { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public string ResponseAsHex => BitConverter.ToString(RawData.ToArray()).Replace("-", " ");

        public override string ToString()
        {
            return IsPositive ?
                $"Positive Response - SID: 0x{ServiceId:X2}" :
                $"Negative Response - SID: 0x{ServiceId:X2}, NRC: 0x{NegativeResponseCode:X2}";
        }
    }
}
