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
        private bool _isConnected;
        private ConnectionSettings _currentSettings;
        private readonly Random _random = new Random(); // Simulate responses for demo

        public bool IsConnected => _isConnected;

        public event EventHandler<UdsResponse> ResponseReceived;

        public Task<bool> ConnectAsync(ConnectionSettings settings)
        {
            try
            {
                // In a real implementation, this would establish a connection with the hardware interface
                // For now, we'll just simulate a connection
                _currentSettings = settings;
                _isConnected = true;

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection error: {ex.Message}");
                _isConnected = false;
                return Task.FromResult(false);
            }
        }

        public Task<bool> DisconnectAsync()
        {
            try
            {
                // Close the connection to the hardware interface
                _isConnected = false;
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Disconnection error: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        public async Task<UdsResponse> SendCommandAsync(UdsCommand command, CancellationToken cancellationToken = default)
        {
            if (!_isConnected)
                throw new InvalidOperationException("Not connected to ECU");

            var response = new UdsResponse
            {
                ServiceId = (byte)(command.ServiceId | 0x40),  // Positive response has 0x40 added to SID
                Timestamp = DateTime.Now
            };

            // Simulate communication delay
            await Task.Delay(_random.Next(50, 300), cancellationToken);

            // Randomly generate positive or negative response for demo purposes
            bool isPositive = _random.Next(0, 10) > 1; // 80% chance of positive response

            if (isPositive)
            {
                // Create a simulated positive response
                var responseData = new List<byte> { response.ServiceId };

                if (command.Subfunction.HasValue)
                    responseData.Add(command.Subfunction.Value);

                if (command.Did.HasValue)
                {
                    responseData.Add((byte)((command.Did.Value >> 8) & 0xFF));
                    responseData.Add((byte)(command.Did.Value & 0xFF));
                }

                // Add some random data for read-type services
                if (command.ServiceId == 0x22)
                {
                    for (int i = 0; i < _random.Next(1, 10); i++)
                        responseData.Add((byte)_random.Next(0, 256));
                }

                response.RawData = responseData;
            }
            else
            {
                // Create a simulated negative response
                response.ServiceId = 0x7F;
                response.NegativeResponseCode = (byte)_random.Next(0x10, 0x40);
                response.RawData = new List<byte> { 0x7F, command.ServiceId, response.NegativeResponseCode };
            }

            // Notify subscribers of the received response
            ResponseReceived?.Invoke(this, response);

            return response;
        }
    }
}
