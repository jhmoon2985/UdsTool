using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UdsTool.Models
{
    // ISO-TP 프레임 타입
    public enum IsoTpFrameType
    {
        SingleFrame,      // SF
        FirstFrame,       // FF
        ConsecutiveFrame, // CF
        FlowControl       // FC
    }

    // ISO-TP 프레임 정보 클래스
    public class IsoTpFrame
    {
        public IsoTpFrameType FrameType { get; set; }
        public DateTime Timestamp { get; set; }
        public byte[] RawData { get; set; }
        public string HexData => BitConverter.ToString(RawData).Replace("-", " ");
        public string Description { get; set; }
        public bool IsTransmitted { get; set; }  // 송신/수신 구분
    }

    // 진단 세션 정보
    public class DiagnosticSession
    {
        public string SessionName { get; set; }
        public DateTime StartTime { get; set; }
        public ObservableCollection<IsoTpFrame> Frames { get; set; } = new ObservableCollection<IsoTpFrame>();
        public IsoTpConfig IsoTpConfig { get; set; } = new IsoTpConfig();

        public DiagnosticSession()
        {
            StartTime = DateTime.Now;
            SessionName = $"Session_{StartTime:yyyyMMdd_HHmmss}";
        }
    }
}
