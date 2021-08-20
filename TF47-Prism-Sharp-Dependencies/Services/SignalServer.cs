using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using TF47_Prism_Sharp_Dependencies.NetworkPackages;

namespace TF47_Prism_Sharp_Dependencies.Services
{
    public class ServerInstance
    {
        public Guid ServerId;
        public readonly IClientProxy Server;
        public readonly string ServerConnectionId;
        public readonly List<ClientInstance> ClientInstances = new();

        public List<ArmaNetworkMessage> JipQueue = new();
        
        public ServerInstance(IClientProxy server, Guid serverId, string serverConnectionId)
        {
            ServerId = serverId;
            Server = server;
            ServerConnectionId = serverConnectionId;
        }

        public ServerInstance AddClientInstance(ClientInstance clientInstance)
        {
            clientInstance.ServerInstance = this;
            ClientInstances.Add(clientInstance);
            return this;
        }
    }

    public class ClientInstance
    {
        public readonly IClientProxy Client;
        public string OwnerId;
        public string ClientConnectionId;
        public string IpAddress;
        public ServerInstance ServerInstance;

        public ClientInstance(IClientProxy client, string ownerId, string clientConnectionId, string ipAddress)
        {
            Client = client;
            OwnerId = ownerId;
            ClientConnectionId = clientConnectionId;
            IpAddress = ipAddress;
        }
    }
    
    
    public class SignalServer : Hub
    {
        private readonly LoggerService _loggerService;
        private readonly HashSet<string> _allowedServerIps = new();
        private readonly HashSet<ServerInstance> _serverInstances = new();

        public SignalServer(IConfiguration configuration, LoggerService loggerService)
        {
            _loggerService = loggerService;
            var addresses = configuration.GetSection("AllowedServerIPs").Get<string[]>();
            _allowedServerIps.UnionWith(addresses);
        }
        
        public override async Task OnConnectedAsync()
        {
            var connectionInfo = Context.Features.Get<IHttpConnectionFeature>();
            _loggerService.LogInformation($"Client {Context.ConnectionId}:{connectionInfo.RemoteIpAddress} established connection to the server...");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            //first handle case that the server crashes or unexpectedly had a disconnect
            var serverInstance = _serverInstances
                .FirstOrDefault(x => x.ServerConnectionId == Context.ConnectionId);
            if (serverInstance != null)
            {
                await Clients.Group(serverInstance.ServerId.ToString()).SendAsync("serverDisconnected");
                return;
            }

            serverInstance = _serverInstances
                .FirstOrDefault(x => x.ClientInstances.Any(x => x.ClientConnectionId == Context.ConnectionId));
            var clientInstance =
                serverInstance?.ClientInstances.FirstOrDefault(x => x.ClientConnectionId == Context.ConnectionId);

            if (clientInstance == null)
            {
                _loggerService.LogWarning($"Could not locate disconnecting client: {Context.ConnectionId}");
                return;
            }

            clientInstance.ServerInstance.ClientInstances.Remove(clientInstance);
            await Groups.RemoveFromGroupAsync(clientInstance.ClientConnectionId, serverInstance.ServerId.ToString());
            

        }

        public async Task CreateServer()
        {
            var connectionInfo = Context.Features.Get<IHttpConnectionFeature>();
            if (!_allowedServerIps.Contains(connectionInfo.RemoteIpAddress.ToString()))
            {
                _loggerService.LogWarning(
                    $"{connectionInfo.ConnectionId} with IP: {connectionInfo.RemoteIpAddress} tried to access protected endpoint");
                return;
            }

            var serverId = Guid.NewGuid();
            
            if (_serverInstances.Add(new ServerInstance(Clients.Caller, serverId, Context.ConnectionId)))
                await Clients.Caller.SendAsync("serverCreated", serverId);

            await Groups.AddToGroupAsync(Context.ConnectionId, serverId.ToString());
            
            _loggerService.LogInformation($"Server created with instance name: {serverId}");
        }

        public async Task RegisterClient(string serverId, string ownerId)
        {
            var connectionInfo = Context.Features.Get<IHttpConnectionFeature>();
            _loggerService.LogInformation($"Client {ownerId}:{connectionInfo.RemoteIpAddress} tries to register to server {serverId}...");
            
            var serverGuid = Guid.Parse(serverId);

            var serverInstance = _serverInstances.FirstOrDefault(x => x.ServerId == serverGuid);
            if (serverInstance == null)
            {
                _loggerService.LogWarning($"Client {Context.ConnectionId}:{connectionInfo.RemoteIpAddress} tried to connect to server that does not exist!");
                return;
            }
            
            serverInstance.ClientInstances.Add(new ClientInstance(Clients.Caller, ownerId, Context.ConnectionId, connectionInfo.RemoteIpAddress.ToString()));
            await Groups.AddToGroupAsync(Context.ConnectionId, serverId);
        }

        [HubMethodName("updateArmaValue")]
        public async Task HandleArmaNetworkMessage(ArmaNetworkMessage armaNetworkMessage)
        {
            var serverInstance = _serverInstances.FirstOrDefault(x => x.ServerId == armaNetworkMessage.ServerId);
            var taskList = new List<Task>(200);
            
            if (serverInstance == null)
            {
                var connectionInfo = Context.Features.Get<IHttpConnectionFeature>();
                _loggerService.LogWarning(
                    $"Client {armaNetworkMessage.SenderId}:{connectionInfo.RemoteIpAddress} tried to updating arma values but server: {armaNetworkMessage.ServerId} does not exist.");
                return;
            }
            
            switch (armaNetworkMessage.TargetId)
            {
                case "-2":
                {
                    taskList.AddRange(serverInstance.ClientInstances
                        .Where(clientInstance => clientInstance.OwnerId != armaNetworkMessage.SenderId)
                        .Select(clientInstance => Clients.User(clientInstance.ClientConnectionId)
                            .SendAsync("updateArmaValue", armaNetworkMessage)));
                    return;
                }
                case "0":
                {
                    taskList.AddRange(serverInstance.ClientInstances
                        .Where(clientInstance => clientInstance.OwnerId != armaNetworkMessage.SenderId)
                        .Select(clientInstance => Clients.User(clientInstance.ClientConnectionId)
                            .SendAsync("updateArmaValue", armaNetworkMessage)));

                    if (armaNetworkMessage.SenderId != "2")
                    {
                        taskList.Add(Clients.User(serverInstance.ServerConnectionId)
                            .SendAsync("updateArmaValue", armaNetworkMessage));
                    }

                    return;
                }
                case "2":
                {
                    taskList.Add(Clients.User(serverInstance.ServerConnectionId)
                        .SendAsync("updateArmaValue", armaNetworkMessage));
                    return;
                }
                default:
                {
                    var clientInstance = serverInstance.ClientInstances.FirstOrDefault(x => x.OwnerId == armaNetworkMessage.TargetId);
                    if (clientInstance == null)
                    {
                        var connectionInfo = Context.Features.Get<IHttpConnectionFeature>();
                        _loggerService.LogWarning(
                            $"Client {armaNetworkMessage.SenderId}:{connectionInfo.RemoteIpAddress} tried to updating " +
                            $"arma values for a client, but client {armaNetworkMessage.TargetId} does not exist.");
                        return;
                    }
                    
                    taskList.Add(Clients.User(clientInstance.ClientConnectionId).SendAsync("updateArmaValue", armaNetworkMessage));
                    break;
                }
            }

            try
            {
                await Task.WhenAll(taskList);
            }
            catch (Exception ex)
            {
                _loggerService.LogError($"Failed to update arma values to at least one client. Task list awaiting completion failed in 3sec window.");
            }
        }
    }
}