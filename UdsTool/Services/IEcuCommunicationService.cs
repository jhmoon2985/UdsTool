using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UdsTool.Models;

namespace UdsTool.Services
{
    public interface IEcuCommunicationService
    {
        event EventHandler<IsoTpFrame> FrameReceived;
        bool IsConnected { get; }
        IsoTpConfig CurrentConfig { get; set; }

        Task<bool> ConnectAsync(string portName);
        Task DisconnectAsync();
        Task<byte[]> SendRequestAsync(byte[] data);
        Task SendRawFrameAsync(byte[] data, uint canId);
    }
}
