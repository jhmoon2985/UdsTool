using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UdsTool.Core.Interfaces;
using UdsTool.Core.Models;

namespace UdsTool.Services
{
    public class CanMessage : ICanMessage
    {
        public uint CanId { get; set; }
        public byte[] Data { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsExtended { get; set; }
        public bool IsRemote { get; set; }
        public bool IsFd { get; set; }

        public CanMessage()
        {
            Timestamp = DateTime.Now;
            Data = new byte[8];
        }

        public CanMessage(uint canId, byte[] data, bool isExtended = false, bool isRemote = false, bool isFd = false)
        {
            CanId = canId;
            Data = data ?? new byte[8];
            Timestamp = DateTime.Now;
            IsExtended = isExtended;
            IsRemote = isRemote;
            IsFd = isFd;
        }

        public override string ToString()
        {
            return $"{Timestamp:HH:mm:ss.fff} | {(IsExtended ? "29bit" : "11bit")} | {CanId:X3} | {BitConverter.ToString(Data).Replace("-", " ")}";
        }
    }

    public class UdsService : IUdsService
    {
        private ICanService _canService;
        private IsoTpConfig _configuration;
        private TaskCompletionSource<byte[]> _responseCompletionSource;
        private CancellationTokenSource _cancellationTokenSource;
        private List<byte[]> _multiFrameBuffer;
        private int _expectedLength;
        private byte _expectedSequenceNumber;

        public event EventHandler<byte[]> ResponseReceived;
        public event EventHandler<string> MessageLogged;

        public IsoTpConfig Configuration
        {
            get => _configuration;
            set => _configuration = value;
        }

        public UdsService()
        {
            _configuration = new IsoTpConfig();
            _multiFrameBuffer = new List<byte[]>();
        }

        byte[] ParseSingleFrame(ICanMessage message)
        {

        }

        public async Task<bool> InitializeAsync(ICanService canService)
        {
            if (canService == null)
                throw new ArgumentNullException(nameof(canService));

            _canService = canService;
            _canService.MessageReceived += OnCanMessageReceived;
            return true;
        }

        public void Shutdown()
        {
            if (_canService != null)
            {
                _canService.MessageReceived -= OnCanMessageReceived;
                _canService = null;
            }

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        public async Task<byte[]> SendRequestAsync(byte[] request)
        {
            if (_canService == null || !_canService.IsConnected)
                throw new InvalidOperationException("CAN service is not connected");

            if (request == null || request.Length == 0)
                throw new ArgumentException("Request data cannot be null or empty");

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            _responseCompletionSource = new TaskCompletionSource<byte[]>();

            _multiFrameBuffer.Clear();
            _expectedSequenceNumber = 1;  // Reset for new transaction

            try
            {
                // Select appropriate ISO-TP frame type based on data length
                if (request.Length <= 7)
                {
                    // Use Single Frame
                    var sfMessage = BuildSingleFrame(request);
                    LogMessage($"TX SF: {BitConverter.ToString(sfMessage.Data)}");
                    await _canService.SendMessageAsync(sfMessage);
                }
                else
                {
                    // Use Multi Frame (First Frame + Consecutive Frames)
                    var ffMessage = BuildFirstFrame(request);
                    LogMessage($"TX FF: {BitConverter.ToString(ffMessage.Data)}");
                    await _canService.SendMessageAsync(ffMessage);

                    // Wait for Flow Control from receiver before sending CF
                    // This will be handled in the MessageReceived event
                }

                // Set timeout for the response
                var timeoutTask = Task.Delay(_configuration.Timeout, _cancellationTokenSource.Token);
                var completedTask = await Task.WhenAny(_responseCompletionSource.Task, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    _responseCompletionSource.TrySetException(new TimeoutException("UDS request timed out"));
                    return null;
                }

                return await _responseCompletionSource.Task;
            }
            catch (Exception ex)
            {
                _responseCompletionSource.TrySetException(ex);
                LogMessage($"Error sending UDS request: {ex.Message}");
                return null;
            }
        }

        private void OnCanMessageReceived(object sender, ICanMessage message)
        {
            // Process only messages with the configured response CAN ID
            if (message.CanId != _configuration.ResponseCanId)
                return;

            try
            {
                // Determine the ISO-TP frame type based on first nibble
                var frameType = (message.Data[0] >> 4) & 0x0F;

                switch (frameType)
                {
                    case 0: // Single Frame
                        var sfData = ParseSingleFrame(message);
                        LogMessage($"RX SF: {BitConverter.ToString(sfData)}");
                        _responseCompletionSource?.TrySetResult(sfData);
                        ResponseReceived?.Invoke(this, sfData);
                        break;

                    case 1: // First Frame
                        var ffResult = ParseFirstFrame(message);
                        if (ffResult.isFirstFrame)
                        {
                            LogMessage($"RX FF: Length={ffResult.dataLength}, Data={BitConverter.ToString(ffResult.data)}");
                            _expectedLength = ffResult.dataLength;
                            _multiFrameBuffer.Clear();
                            _multiFrameBuffer.Add(ffResult.data);
                            _expectedSequenceNumber = 1;

                            // Send Flow Control
                            var fcMessage = BuildFlowControl(0, _configuration.BlockSize, _configuration.SeparationTime);
                            _canService.SendMessageAsync(fcMessage).ConfigureAwait(false);
                            LogMessage($"TX FC: BS={_configuration.BlockSize}, ST={_configuration.SeparationTime}");
                        }
                        break;

                    case 2: // Consecutive Frame
                        var cfResult = ParseConsecutiveFrame(message);
                        if (cfResult.isConsecutiveFrame)
                        {
                            LogMessage($"RX CF: SN={cfResult.sequenceNumber}, Data={BitConverter.ToString(cfResult.data)}");

                            // Check sequence number
                            if (cfResult.sequenceNumber != _expectedSequenceNumber)
                            {
                                LogMessage($"Sequence error: Expected {_expectedSequenceNumber}, got {cfResult.sequenceNumber}");
                                _responseCompletionSource?.TrySetException(new Exception("ISO-TP sequence error"));
                                return;
                            }

                            _multiFrameBuffer.Add(cfResult.data);
                            _expectedSequenceNumber = (byte)((_expectedSequenceNumber + 1) % 16);  // Wrap at 15

                            // Check if we have received all frames
                            var totalReceived = _multiFrameBuffer.Sum(b => b.Length);
                            if (totalReceived >= _expectedLength)
                            {
                                // Combine all frames into one response
                                var completeResponse = new byte[_expectedLength];
                                var offset = 0;

                                foreach (var frame in _multiFrameBuffer)
                                {
                                    var bytesToCopy = Math.Min(frame.Length, _expectedLength - offset);
                                    Array.Copy(frame, 0, completeResponse, offset, bytesToCopy);
                                    offset += bytesToCopy;

                                    if (offset >= _expectedLength)
                                        break;
                                }

                                LogMessage($"RX Complete: {BitConverter.ToString(completeResponse)}");
                                _responseCompletionSource?.TrySetResult(completeResponse);
                                ResponseReceived?.Invoke(this, completeResponse);
                            }
                        }
                        break;

                    case 3: // Flow Control
                        var fcResult = ParseFlowControl(message);
                        if (fcResult.isFlowControl)
                        {
                            LogMessage($"RX FC: Status={fcResult.flowStatus}, BS={fcResult.blockSize}, ST={fcResult.separationTime}");
                            // Handle flow control in the send logic
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error processing CAN message: {ex.Message}");
                _responseCompletionSource?.TrySetException(ex);
            }
        }