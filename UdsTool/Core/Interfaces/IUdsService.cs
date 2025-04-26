using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UdsTool.Core.Models;
using UdsTool.ViewModels;

namespace UdsTool.Core.Interfaces
{

    public interface IUdsService
    {
        event EventHandler<byte[]> ResponseReceived;
        event EventHandler<string> MessageLogged;

        IsoTpConfig Configuration { get; set; }

        Task<byte[]> SendRequestAsync(byte[] request);
        Task<bool> InitializeAsync(ICanService canService);
        void Shutdown();

        // ISO-TP Frame Parsing Methods
        byte[] ParseSingleFrame(ICanMessage message);
        (bool isFirstFrame, int dataLength, byte[] data) ParseFirstFrame(ICanMessage message);
        (bool isFlowControl, byte flowStatus, byte blockSize, byte separationTime) ParseFlowControl(ICanMessage message);
        (bool isConsecutiveFrame, byte sequenceNumber, byte[] data) ParseConsecutiveFrame(ICanMessage message);

        // ISO-TP Frame Building Methods
        ICanMessage BuildSingleFrame(byte[] data);
        ICanMessage BuildFirstFrame(byte[] data);
        ICanMessage BuildFlowControl(byte flowStatus, byte blockSize, byte separationTime);
        ICanMessage BuildConsecutiveFrame(byte sequenceNumber, byte[] data);
    }
}