using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TF47_Prism_Sharp_Dependencies.NetworkPackages;
using TF47_Prism_Sharp_Dependencies.Services;

namespace TF47_Prism_Sharp_Client.Services
{
    public class MediatorService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SignalClient _signalClient;
        private readonly ArmaCallbackService _callbackService;
        private Guid _serverId;

        public MediatorService(IServiceProvider serviceProvider, SignalClient signalClient)
        {
            _serviceProvider = serviceProvider;
            _signalClient = signalClient;
            _signalClient.UpdateArmaValue += SignalClientArmaValueUpdated;
            unsafe
            {
                _callbackService = new ArmaCallbackService(Extension.Callback).StartEngineCallback();
            }
        }

        ~MediatorService()
        {
        }

        public void UpdateArmaValue(string @namespace, string variableName, string data, bool isJip, string ownerId, string targetId)
        {
            _signalClient.SendNetworkMessage(new ArmaNetworkMessage
            {
                Data = data,
                IsJip = isJip,
                Namespace = @namespace,
                SenderId = ownerId,
                ServerId = _serverId,
                TargetId = targetId,
                VariableName = variableName
            });
        }
        

        private int CallbackEngine(string function, string data)
        {
            unsafe
            {
                try
                {
                     return Extension.Callback("TF47PrismSharp", function, data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to call callback delegate function: {function} | data: {data}");
                }
            }

            return -1;
        }
        
        private void SignalClientArmaValueUpdated(ArmaNetworkMessage armaNetworkMessage)
        {
            _callbackService.EnqueueEngineCallback("updateArmaValue", $"[\"{armaNetworkMessage.Namespace},{armaNetworkMessage.VariableName},{armaNetworkMessage.Data}]");
        }

    }
}