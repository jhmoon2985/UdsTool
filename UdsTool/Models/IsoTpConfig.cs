using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UdsTool.Models
{
    public class IsoTpConfig
    {
        // ISO-TP Flow Control 설정
        public int BlockSize { get; set; } = 0;  // 0: 제한 없음
        public int STmin { get; set; } = 0;      // Separation Time minimum (ms)

        // Frame 관련 설정
        public int MaxFrameSize { get; set; } = 8;
        public int FirstFrameDataSize { get; set; } = 6;
        public int ConsecutiveFrameDataSize { get; set; } = 7;

        // 타임아웃 설정
        public int TimeoutMs { get; set; } = 1000;
        public int FlowControlTimeoutMs { get; set; } = 1000;

        // CAN ID 설정
        public uint RequestId { get; set; } = 0x7E0;
        public uint ResponseId { get; set; } = 0x7E8;
    }
}
