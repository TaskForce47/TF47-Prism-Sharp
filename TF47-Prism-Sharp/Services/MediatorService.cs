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
        private readonly DataServer _dataServer;
        private readonly ThreadSafeQueue<(string, bool)> _whitelistUpdateRequests = new();
        private readonly CancellationTokenSource _backgroundWorkerToken = new();

        public MediatorService(IServiceProvider serviceProvider, DataServer dataServer)
        {
            _serviceProvider = serviceProvider;
            _dataServer = dataServer;
            Task.Run(async () => { await RunBackgroundTask(_backgroundWorkerToken.Token); }).ConfigureAwait(false);
        }

        ~MediatorService()
        {
            _backgroundWorkerToken.Cancel();
        }

        public void ClientConnected(string ownerId)
        {
            
        }
        
        public void UpdatePlayerPermissions(string playerUid, bool firstLoad)
        {
            //max wait 100ms to enqueue task to prevent notable server freezing
            CancellationTokenSource cts = new(TimeSpan.FromMilliseconds(100));
            try
            {
                _whitelistUpdateRequests.EnqueueAsync((playerUid, firstLoad), cts.Token).Wait();
            }
            catch
            {
                // only happening if something is terrible wrong with queue
            }
        }

        public void UpdateTicketCount(int ticketChange, int ticketCountNew, string message, string playerUid)
        {
            Task.Run(async ()=>
            {
                
                var scope = _serviceProvider.CreateScope();
                var apiClient = scope.ServiceProvider.GetRequiredService<ApiClient>();
                
                await apiClient.UpdateTicketCountAsync(playerUid, ticketChange, ticketCountNew, message, _backgroundWorkerToken.Token);
            }, _backgroundWorkerToken.Token).ConfigureAwait(false);
        }

        public int CreateSession(string worldName, string missionType, int missionId)
        {
            var scope = _serviceProvider.CreateScope();
            var apiClient = scope.ServiceProvider.GetRequiredService<ApiClient>();
            
            CancellationTokenSource cts = new(TimeSpan.FromMilliseconds(1000*4));
            var sessionId = apiClient.CreateSessionAsync(worldName, missionType, missionId, cts.Token).Result;
            return sessionId;
        }

        public bool EndSession()
        {
            var scope = _serviceProvider.CreateScope();
            var apiClient = scope.ServiceProvider.GetRequiredService<ApiClient>();
            
            CancellationTokenSource cts = new(TimeSpan.FromMilliseconds(1000*4));
            var successful = apiClient.EndSessionAsync(cts.Token).Result;
            if (!successful)
                Console.WriteLine("Failed stop current session");

            return successful;
        }

        public void CreateUser(string playerUid, string username)
        {
            Task.Run(async () =>
            {
                var scope = _serviceProvider.CreateScope();
                var apiClient = scope.ServiceProvider.GetRequiredService<ApiClient>();

                if (!await apiClient.CheckUserExistsAsync(playerUid, _backgroundWorkerToken.Token))
                {
                    await apiClient.CreateUserAsync(playerUid, username, _backgroundWorkerToken.Token);
                }
            }, _backgroundWorkerToken.Token).ConfigureAwait(false);
        }

        private async Task RunBackgroundTask(CancellationToken cancellationToken)
        {
            var scope = _serviceProvider.CreateScope();
            var apiClient = scope.ServiceProvider.GetRequiredService<ApiClient>();
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken);
                while (!_whitelistUpdateRequests.IsEmpty && !cancellationToken.IsCancellationRequested)
                {
                    var item = await _whitelistUpdateRequests.DequeueAsync(cancellationToken);
                    var playerWhitelist =
                        (await apiClient.GetPlayerWhitelisting(item.Item1, cancellationToken)).ToArmaArray();
                    playerWhitelist = $"[\"{item.Item1}\",{item.Item2},{playerWhitelist}]"; 

                    //arma allows a max of 99 callbacks per frame
                    //-1 notifies the callback queue is full
                    //we will wait until pushback to the callback queue is successful
                    var id = CallbackEngine("TF47PrismWhitelistUpdate", playerWhitelist);
                    while (id == -1)
                    {
                        await Task.Delay(1, cancellationToken);
                        id = CallbackEngine("TF47PrismWhitelistUpdate", playerWhitelist);
                    }
                }
            }
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

    }
}