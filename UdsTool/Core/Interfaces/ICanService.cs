using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UdsTool.Core.Models;
using UdsTool.ViewModels;

namespace UdsTool.Core.Interfaces
{
    public interface ICanMessage
    {
        uint CanId { get; set; }
        byte[] Data { get; set; }
        DateTime Timestamp { get; set; }
        bool IsExtended { get; set; }
        bool IsRemote { get; set; }
        bool IsFd { get; set; }
    }

    public interface ICanService
    {
        event EventHandler<ICanMessage> MessageReceived;
        bool IsConnected { get; }
        Task<bool> ConnectAsync(string interfaceName);
        Task DisconnectAsync();
        Task SendMessageAsync(ICanMessage message);
    }
}