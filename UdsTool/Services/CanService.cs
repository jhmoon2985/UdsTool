using System;
using System.Threading.Tasks;
using UdsTool.Core.Interfaces;

namespace UdsTool.Services
{
    public class CanService : ICanService
    {
        private bool _isConnected;
        private string _currentInterface;

        public event EventHandler<ICanMessage> MessageReceived;

        public bool IsConnected => _isConnected;

        public async Task<bool> ConnectAsync(string interfaceName)
        {
            // In a real implementation, this would initialize the CAN hardware interface
            // For this example, we'll simulate a successful connection
            await Task.Delay(500); // Simulate hardware initialization

            _currentInterface = interfaceName;
            _isConnected = true;

            // Start a background task to simulate receiving CAN messages
            _ = Task.Run(SimulateCanTrafficAsync);

            return true;
        }

        public async Task DisconnectAsync()
        {
            // In a real implementation, this would disconnect from the CAN hardware
            await Task.Delay(200); // Simulate hardware shutdown

            _isConnected = false;
            _currentInterface = null;
        }

        public async Task SendMessageAsync(ICanMessage message)
        {
            if (!_isConnected)
                throw new InvalidOperationException("CAN interface is not connected");

            // In a real implementation, this would send the message to the CAN bus
            await Task.Delay(10); // Simulate transmission time

            // For debugging purposes, echo the message back as received
            // In a real implementation, we wouldn't do this
            // await Task.Delay(5);
            // OnMessageReceived(message);
        }

        private async Task SimulateCanTrafficAsync()
        {
            // This method simulates CAN traffic for testing
            // In a real implementation, we would listen to the actual CAN bus

            Random random = new Random();

            while (_isConnected)
            {
                await Task.Delay(random.Next(500, 2000)); // Random delay between messages

                if (!_isConnected) break;

                // Create a random CAN message (for demonstration purposes)
                var data = new byte[8];
                random.NextBytes(data);

                var canId = (uint)random.Next(0x100, 0x7FF); // Random standard ID

                var message = new CanMessage
                {
                    CanId = canId,
                    Data = data,
                    Timestamp = DateTime.Now,
                    IsExtended = false,
                    IsRemote = false,
                    IsFd = false
                };

                // Don't fire the event if we've been disconnected during the delay
                if (_isConnected)
                    OnMessageReceived(message);
            }
        }

        protected virtual void OnMessageReceived(ICanMessage message)
        {
            MessageReceived?.Invoke(this, message);
        }
    }
}