using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TF47_Prism_Sharp.Models.Api;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace TF47_Prism_Sharp.Services
{
    public class ApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _client;
        
        public ApiClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _client = _httpClientFactory.CreateClient("api");
        }

        public async Task<bool> CheckUserExistsAsync(string playerUid, CancellationToken cancellationToken)
        {
            var route = $"/api/player/{playerUid}";
            try
            {
                var response = await _client.GetAsync(route, HttpCompletionOption.ResponseContentRead, cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while fetching player: {ex.Message}");
            }

            return false;
        }

        public async Task<bool> CreateUserAsync(string playerUid, string playerName, CancellationToken cancellationToken)
        {
            var route = "/api/player";
            var request = JsonSerializer.Serialize(new CreatePlayerRequest
            {
                PlayerUid = playerUid,
                PlayerName = playerName
            });
            try
            {
                var response =
                    await _client.PostAsync(route, new StringContent(request, Encoding.UTF8, "application/json"),
                        cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while trying to create new user: {ex.Message}");
            }

            return false;
        }

        public async Task<int> CreateSessionAsync(string worldName, string missionType, int missionId,
            CancellationToken cancellationToken)
        {
            var route = "/api/Session";
            var request = JsonSerializer.Serialize(new CreateSessionRequest
            {
                MissionId = missionId,
                MissionType = missionType,
                WorldName = worldName
            });
            Console.WriteLine(request);
            try
            {
                var response =
                    await _client.PostAsync(route, new StringContent(request, Encoding.UTF8, "application/json"),
                        cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var sessionResponse = JsonConvert.DeserializeObject<SessionResponse>(
                        await response.Content.ReadAsStringAsync(cancellationToken));
                    Configuration.SessionId = sessionResponse.SessionId;
                    return sessionResponse.SessionId;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create new session: {ex.Message}");
            }
            return -1;
        }

        public async Task<bool> EndSessionAsync(CancellationToken cancellationToken)
        {
            if (Configuration.SessionId == -1) return true;
            var route = $"/api/Session/{Configuration.SessionId}/endsession";

            try
            {
                var response = await _client.PutAsync(route, new StringContent("", Encoding.UTF8, "application/json"),
                    cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to stop current session: {ex.Message}");
            }

            return false;
        }

        public async Task<bool> UpdateTicketCountAsync(string playerUid, int ticketChange, int ticketCountNew,
            string message, CancellationToken cancellationToken)
        {
            var route = $"/api/ticket/{Configuration.SessionId}";
            var request = JsonSerializer.Serialize(new TicketUpdateRequest
            {
                PlayerUid = playerUid,
                TicketChange = ticketChange,
                TicketCountNew = ticketCountNew,
                Message = message
            });

            try
            {
                var response = await _client.PostAsync(route,
                    new StringContent(request, Encoding.UTF8, "application/json"), cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to update current ticket count: {ex.Message}");
            }

            return false;
        }

        public async Task<List<int>> GetPlayerWhitelisting(string playerUid, CancellationToken cancellationToken)
        {
            var route = $"/api/Whitelist/user/{playerUid}";
            var permissions = new List<int>();
            try
            {
                var response = await _client.GetAsync(route, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<PlayerWhitelistingResponse>(await response.Content.ReadAsStringAsync(cancellationToken));
                    
                    if (result?.Whitelistings == null)
                    {
                        Console.WriteLine($"Failed to fetch permissions! Empty response for playerUid: {playerUid}");
                        return permissions;
                    }
                    
                    foreach (var resultWhitelisting in result.Whitelistings)
                    {
                        permissions.Add(resultWhitelisting.WhitelistId);    
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get player whitelist: {ex.Message}");
            }
            return permissions;
        }
    }
}