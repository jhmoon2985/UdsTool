using System;
using System.Collections.Generic;
using System.Linq;
using UdsTool.Models;

namespace UdsTool.Services
{
    public class EcuCommunicationService : IEcuCommunicationService
    {
        private IsoTpSettings _settings;
        private bool _isConnected;

        public event EventHandler<string> DataReceived;

        public bool IsConnected => _isConnected;

        public void Connect()
        {
            // CAN 연결 구현 (실제 구현에서는 CAN 드라이버 사용)
            _isConnected = true;
        }

        public void Disconnect()
        {
            _isConnected = false;
        }

        public void SendRequest(DiagnosticFrame frame)
        {
            if (!_isConnected)
                throw new InvalidOperationException("Not connected to ECU");

            // ISO-TP 프로토콜 구현
            var canFrames = CreateCanFrames(frame);
            foreach (var canFrame in canFrames)
            {
                // CAN 프레임 전송 구현
                SendCanFrame(canFrame);
            }
        }

        public void UpdateIsoTpSettings(IsoTpSettings settings)
        {
            _settings = settings;
        }

        private List<byte[]> CreateCanFrames(DiagnosticFrame frame)
        {
            var frames = new List<byte[]>();
            var data = new List<byte>();

            data.Add(frame.ServiceId);
            if (frame.SubFunction != 0)
                data.Add(frame.SubFunction);

            if (frame.DataIdentifier != 0)
            {
                data.Add((byte)(frame.DataIdentifier >> 8));
                data.Add((byte)(frame.DataIdentifier & 0xFF));
            }

            if (frame.Data != null)
                data.AddRange(frame.Data);

            // Single Frame
            if (data.Count <= 7)
            {
                var canFrame = new byte[8];
                canFrame[0] = (byte)data.Count;
                data.CopyTo(canFrame, 1);
                frames.Add(canFrame);
            }
            // Multi Frame
            else
            {
                // First Frame
                var firstFrame = new byte[8];
                firstFrame[0] = (byte)(0x10 | ((data.Count >> 8) & 0x0F));
                firstFrame[1] = (byte)(data.Count & 0xFF);
                data.Take(6).ToArray().CopyTo(firstFrame, 2);
                frames.Add(firstFrame);

                // Consecutive Frames
                int remainingBytes = data.Count - 6;
                int index = 6;
                byte sequenceNumber = 1;

                while (remainingBytes > 0)
                {
                    var consecFrame = new byte[8];
                    consecFrame[0] = (byte)(0x20 | (sequenceNumber & 0x0F));

                    int bytesToCopy = Math.Min(7, remainingBytes);
                    data.Skip(index).Take(bytesToCopy).ToArray().CopyTo(consecFrame, 1);

                    frames.Add(consecFrame);

                    remainingBytes -= bytesToCopy;
                    index += bytesToCopy;
                    sequenceNumber = (byte)((sequenceNumber + 1) & 0x0F);
                }
            }

            return frames;
        }

        private void SendCanFrame(byte[] frame)
        {
            // 실제 CAN 프레임 전송 구현
            DataReceived?.Invoke(this, $"Sent: {BitConverter.ToString(frame)}");
        }

        private void OnDataReceived(byte[] data)
        {
            // CAN 데이터 수신 처리
            DataReceived?.Invoke(this, $"Received: {BitConverter.ToString(data)}");

            // ISO-TP 프레임 파싱
            if ((data[0] & 0xF0) == 0x00) // Single Frame
            {
                int length = data[0] & 0x0F;
                ProcessReceivedData(data.Skip(1).Take(length).ToArray());
            }
            else if ((data[0] & 0xF0) == 0x10) // First Frame
            {
                int length = ((data[0] & 0x0F) << 8) | data[1];
                // Flow Control 전송
                SendFlowControl();
            }
            else if ((data[0] & 0xF0) == 0x20) // Consecutive Frame
            {
                // 데이터 조립
            }
            else if ((data[0] & 0xF0) == 0x30) // Flow Control
            {
                // Flow Control 처리
            }
        }

        private void SendFlowControl()
        {
            var flowControl = new byte[8];
            flowControl[0] = 0x30; // Flow Control
            flowControl[1] = _settings.BlockSize;
            flowControl[2] = _settings.SeparationTime;
            SendCanFrame(flowControl);
        }

        private void ProcessReceivedData(byte[] data)
        {
            // 수신된 데이터 처리
            byte serviceId = data[0];
            string response = $"Response: Service ID = 0x{serviceId:X2}";

            if (data.Length > 1)
            {
                byte subFunction = data[1];
                response += $", SubFunction = 0x{subFunction:X2}";
            }

            DataReceived?.Invoke(this, response);
        }
    }
}