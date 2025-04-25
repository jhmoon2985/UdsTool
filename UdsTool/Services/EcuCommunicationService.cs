using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UdsTool.Models;

namespace UdsTool.Services
{
    public class EcuCommunicationService : IEcuCommunicationService
    {
        public event EventHandler<IsoTpFrame> FrameReceived;
        public bool IsConnected { get; private set; }
        public IsoTpConfig CurrentConfig { get; set; } = new IsoTpConfig();

        // 실제 구현에서는 CAN 통신을 위한 드라이버/라이브러리 사용 필요
        // 여기서는 간단한 구현만 제공합니다

        public async Task<bool> ConnectAsync(string portName)
        {
            // 실제 연결 로직 구현
            await Task.Delay(100);  // 통신 지연 시뮬레이션
            IsConnected = true;
            return true;
        }

        public async Task DisconnectAsync()
        {
            // 연결 해제 로직
            await Task.Delay(100);
            IsConnected = false;
        }

        public async Task<byte[]> SendRequestAsync(byte[] data)
        {
            if (!IsConnected)
                throw new InvalidOperationException("CAN 통신이 연결되지 않았습니다.");

            // 요청 데이터 처리 및 ISO-TP 프레임 구성
            var frames = PackIsoTpFrames(data);

            // 프레임 전송 및 응답 수신 처리
            foreach (var frame in frames)
            {
                await SendFrame(frame);
            }

            // 응답 수신 시뮬레이션 (실제 구현에서는 응답을 기다리고 처리해야 함)
            var responseData = SimulateResponse(data);
            return responseData;
        }

        public async Task SendRawFrameAsync(byte[] data, uint canId)
        {
            if (!IsConnected)
                throw new InvalidOperationException("CAN 통신이 연결되지 않았습니다.");

            await Task.Delay(50);  // 통신 지연 시뮬레이션

            // 실제 구현에서는 CAN ID와 함께 데이터 전송
        }

        // ISO-TP 프레임 패킹 메서드
        private List<IsoTpFrame> PackIsoTpFrames(byte[] data)
        {
            var frames = new List<IsoTpFrame>();

            if (data.Length <= CurrentConfig.MaxFrameSize - 1)
            {
                // Single Frame (SF)
                byte[] sfData = new byte[data.Length + 1];
                sfData[0] = (byte)data.Length;  // First byte = length
                Array.Copy(data, 0, sfData, 1, data.Length);

                frames.Add(new IsoTpFrame
                {
                    FrameType = IsoTpFrameType.SingleFrame,
                    RawData = sfData,
                    Timestamp = DateTime.Now,
                    IsTransmitted = true,
                    Description = "Single Frame"
                });
            }
            else
            {
                // First Frame (FF)
                byte[] ffData = new byte[CurrentConfig.MaxFrameSize];
                ushort length = (ushort)data.Length;
                ffData[0] = (byte)(0x10 | (length >> 8));   // 첫 니블: 1, 두 번째 니블: 상위 4비트
                ffData[1] = (byte)(length & 0xFF);          // 하위 8비트

                int firstFrameDataSize = CurrentConfig.FirstFrameDataSize;
                Array.Copy(data, 0, ffData, 2, firstFrameDataSize);

                frames.Add(new IsoTpFrame
                {
                    FrameType = IsoTpFrameType.FirstFrame,
                    RawData = ffData,
                    Timestamp = DateTime.Now,
                    IsTransmitted = true,
                    Description = "First Frame"
                });

                // 나머지 데이터를 Consecutive Frames로 분할
                int remainingBytes = data.Length - firstFrameDataSize;
                int offset = firstFrameDataSize;
                byte seqNum = 1;  // 시퀀스 번호 (1부터 시작, 최대 15까지)

                while (remainingBytes > 0)
                {
                    byte[] cfData = new byte[CurrentConfig.MaxFrameSize];
                    cfData[0] = (byte)(0x20 | (seqNum & 0x0F));  // 2x: CF, x: 시퀀스 번호

                    int bytesToCopy = Math.Min(remainingBytes, CurrentConfig.ConsecutiveFrameDataSize);
                    Array.Copy(data, offset, cfData, 1, bytesToCopy);

                    frames.Add(new IsoTpFrame
                    {
                        FrameType = IsoTpFrameType.ConsecutiveFrame,
                        RawData = cfData,
                        Timestamp = DateTime.Now,
                        IsTransmitted = true,
                        Description = $"Consecutive Frame #{seqNum}"
                    });

                    remainingBytes -= bytesToCopy;
                    offset += bytesToCopy;
                    seqNum = (byte)((seqNum + 1) & 0x0F);  // 시퀀스 번호 순환 (0-15)
                }
            }

            return frames;
        }

        // 프레임 전송 메서드
        private async Task SendFrame(IsoTpFrame frame)
        {
            // 실제 구현에서는 CAN 버스로 프레임 전송
            await Task.Delay(50);  // 통신 지연 시뮬레이션

            // 이벤트 발생
            FrameReceived?.Invoke(this, frame);

            // FC 프레임을 기다려야 하는 경우 (FF 이후)
            if (frame.FrameType == IsoTpFrameType.FirstFrame)
            {
                // FC 프레임 시뮬레이션
                SimulateFlowControlFrame();
            }
        }

        // Flow Control 프레임 시뮬레이션
        private void SimulateFlowControlFrame()
        {
            // FC 프레임 구성: [0x30, BS, ST_min]
            byte[] fcData = new byte[3];
            fcData[0] = 0x30;  // FC 프레임 타입 (Continue to send)
            fcData[1] = (byte)CurrentConfig.BlockSize;
            fcData[2] = (byte)CurrentConfig.STmin;

            var fcFrame = new IsoTpFrame
            {
                FrameType = IsoTpFrameType.FlowControl,
                RawData = fcData,
                Timestamp = DateTime.Now,
                IsTransmitted = false,  // 수신된 프레임
                Description = "Flow Control"
            };

            // 이벤트 발생 (수신된 FC 프레임)
            FrameReceived?.Invoke(this, fcFrame);
        }

        // 응답 시뮬레이션
        private byte[] SimulateResponse(byte[] request)
        {
            // 실제 구현에서는 ECU로부터 응답을 기다려야 함
            // 여기서는 간단한 응답 시뮬레이션만 제공

            byte[] response;

            if (request.Length > 0)
            {
                // 서비스 ID (첫 번째 바이트)에 0x40을 더해 긍정 응답으로 변환
                byte serviceId = request[0];
                byte respServiceId = (byte)(serviceId + 0x40);

                // 간단한 응답 생성
                response = new byte[request.Length + 2];
                response[0] = respServiceId;

                if (request.Length > 1)
                    Array.Copy(request, 1, response, 1, request.Length - 1);

                // 마지막에 더미 데이터 추가
                response[response.Length - 2] = 0xAA;
                response[response.Length - 1] = 0xBB;
            }
            else
            {
                response = new byte[] { 0x7F, 0x00, 0x10 };  // 일반 오류 응답
            }

            // 응답 데이터를 ISO-TP 프레임으로 시뮬레이션
            SimulateResponseFrames(response);

            return response;
        }

        // 응답 프레임 시뮬레이션
        private void SimulateResponseFrames(byte[] response)
        {
            List<IsoTpFrame> respFrames = new List<IsoTpFrame>();

            if (response.Length <= 7)
            {
                // Single Frame 응답
                byte[] sfData = new byte[response.Length + 1];
                sfData[0] = (byte)response.Length;
                Array.Copy(response, 0, sfData, 1, response.Length);

                respFrames.Add(new IsoTpFrame
                {
                    FrameType = IsoTpFrameType.SingleFrame,
                    RawData = sfData,
                    Timestamp = DateTime.Now,
                    IsTransmitted = false,  // 응답은 수신된 것임
                    Description = "Response: Single Frame"
                });
            }
            else
            {
                // First Frame + Consecutive Frames 응답
                byte[] ffData = new byte[8];
                ushort length = (ushort)response.Length;
                ffData[0] = (byte)(0x10 | (length >> 8));
                ffData[1] = (byte)(length & 0xFF);

                int firstFrameDataSize = 6;
                Array.Copy(response, 0, ffData, 2, firstFrameDataSize);

                respFrames.Add(new IsoTpFrame
                {
                    FrameType = IsoTpFrameType.FirstFrame,
                    RawData = ffData,
                    Timestamp = DateTime.Now,
                    IsTransmitted = false,
                    Description = "Response: First Frame"
                });

                // Flow Control 프레임 전송 (시뮬레이션)
                byte[] fcData = new byte[3];
                fcData[0] = 0x30;  // Continue to send
                fcData[1] = (byte)CurrentConfig.BlockSize;
                fcData[2] = (byte)CurrentConfig.STmin;

                respFrames.Add(new IsoTpFrame
                {
                    FrameType = IsoTpFrameType.FlowControl,
                    RawData = fcData,
                    Timestamp = DateTime.Now,
                    IsTransmitted = true,  // FC는 우리가 전송
                    Description = "Flow Control (to ECU)"
                });

                // 나머지 응답 데이터를 Consecutive Frames으로 수신
                int remainingBytes = response.Length - firstFrameDataSize;
                int offset = firstFrameDataSize;
                byte seqNum = 1;

                while (remainingBytes > 0)
                {
                    byte[] cfData = new byte[8];
                    cfData[0] = (byte)(0x20 | (seqNum & 0x0F));

                    int bytesToCopy = Math.Min(remainingBytes, 7);
                    Array.Copy(response, offset, cfData, 1, bytesToCopy);

                    respFrames.Add(new IsoTpFrame
                    {
                        FrameType = IsoTpFrameType.ConsecutiveFrame,
                        RawData = cfData,
                        Timestamp = DateTime.Now,
                        IsTransmitted = false,  // 응답은 수신된 것임
                        Description = $"Response: Consecutive Frame #{seqNum}"
                    });

                    remainingBytes -= bytesToCopy;
                    offset += bytesToCopy;
                    seqNum = (byte)((seqNum + 1) & 0x0F);
                }
            }

            // 모든 응답 프레임에 대해 이벤트 발생
            foreach (var frame in respFrames)
            {
                FrameReceived?.Invoke(this, frame);
            }
        }
    }
}
