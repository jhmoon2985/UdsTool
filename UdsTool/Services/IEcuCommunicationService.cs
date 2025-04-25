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
        Task<bool> ConnectAsync(ConnectionSettings settings);
        Task<bool> DisconnectAsync();
        Task<UdsResponse> SendCommandAsync(UdsCommand command, CancellationToken cancellationToken = default);
        bool IsConnected { get; }
        event EventHandler<UdsResponse> ResponseReceived;
    }
}
