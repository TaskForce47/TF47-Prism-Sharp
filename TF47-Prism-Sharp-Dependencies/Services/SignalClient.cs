using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using TF47_Prism_Sharp_Dependencies.NetworkPackages;

namespace TF47_Prism_Sharp_Dependencies.Services
{
    public class SignalClient
    {
        private readonly LoggerService _loggerService;
        private HubConnection _connection;

        public event Action<ArmaNetworkMessage> UpdateArmaValue;

        public SignalClient(LoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        public async Task ConnectToServer(string hostname)
        {
            
            _connection = new HubConnectionBuilder()
                .WithUrl($"{hostname}/testhub")
                .Build();

            _connection.Closed += async exception =>
            {
                _loggerService.LogWarning($"SignalR connection dropped: {exception}");
                await _connection.StartAsync();
            };
            _connection.Reconnected += s =>
            {
                _loggerService.LogInformation($"SignalR connection restored, new connectionId: {s}");
                return Task.CompletedTask;
            };

            _connection.On("updateArmaValue", (ArmaNetworkMessage armaNetworkMessage) =>
            {
                OnUpdateArmaValue(armaNetworkMessage);
            });
        }
        
        
        public async Task SendNetworkMessage(ArmaNetworkMessage networkMessage)
        {
            await _connection.SendAsync("updateArmaValue", networkMessage);
        }

        protected virtual void OnUpdateArmaValue(ArmaNetworkMessage obj)
        {
            UpdateArmaValue?.Invoke(obj);
        }
    }
}