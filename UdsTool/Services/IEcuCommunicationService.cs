using System;
using UdsTool.Models;

namespace UdsTool.Services
{
    public interface IEcuCommunicationService
    {
        event EventHandler<string> DataReceived;
        void SendRequest(DiagnosticFrame frame);
        void UpdateIsoTpSettings(IsoTpSettings settings);
        void Connect();
        void Disconnect();
        bool IsConnected { get; }
    }
}