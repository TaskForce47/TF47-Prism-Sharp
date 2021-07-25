using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TF47_Prism_Sharp.Helper;

namespace TF47_Prism_Sharp.Services
{
    public class MediatorService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ThreadSafeQueue<string> _whitelistUpdateRequests = new();
        private readonly CancellationTokenSource _backgroundWorkerToken = new();
        private readonly CancellationToken _cancellationToken = new();

        public MediatorService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            Task.Run(async () => { await RunBackgroundTask(_backgroundWorkerToken.Token); });
        }

        ~MediatorService()
        {
            _backgroundWorkerToken.Cancel();
        }

        public void UpdatePlayerPermissions(string playerUid)
        {
            //max wait 10ms to enqueue task to prevent notable server freezing
            CancellationTokenSource cts = new(TimeSpan.FromMilliseconds(10));
            _whitelistUpdateRequests.EnqueueAsync(playerUid, cts.Token).Wait();
        }

        private async Task RunBackgroundTask(CancellationToken cancellationToken)
        {
            var scope = _serviceProvider.CreateScope();
            var apiClient = scope.ServiceProvider.GetRequiredService<ApiClient>();
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(100 * 10, cancellationToken);
                while (!_whitelistUpdateRequests.IsEmpty && !cancellationToken.IsCancellationRequested)
                {
                    var playerUid = await _whitelistUpdateRequests.DequeueAsync(CancellationToken.None);
                    var result = await apiClient.GetPlayerWhitelisting(playerUid, cancellationToken);
                    
                }
            }
        }

        private void CallbackEngine(string function, string data)
        {
            unsafe
            {
                try
                {
                    Extension.Callback("TF47PrismSharp", function, data);
                }
                catch (Exception ex)
                {
                    
                }
            }
        }

    }
}