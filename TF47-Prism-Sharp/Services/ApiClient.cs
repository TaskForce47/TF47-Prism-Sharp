using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TF47_Prism_Sharp.Models.Api;

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
            var request = JsonSerializer.Serialize(new
            {
                PlayerUid = playerUid,
                PlayerName = playerName
            });
            try
            {
                var response =
                    await _client.PostAsync(route, new StringContent(request, Encoding.UTF8), cancellationToken);
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
            var route = "/api/session";
            var request = JsonSerializer.Serialize(new
            {
                MissionId = missionId,
                MissionType = missionType,
                WorldName = worldName
            });

            try
            {
                var response =
                    await _client.PostAsync(route, new StringContent(request, Encoding.UTF8), cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var sessionResponse = await JsonSerializer.DeserializeAsync<SessionResponse>(
                        await response.Content.ReadAsStreamAsync(cancellationToken),
                        cancellationToken: cancellationToken);
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

        public async Task<bool> UpdateTicketCount(string playerUid, int ticketChange, int ticketCountNew,
            string message, CancellationToken cancellationToken)
        {
            var route = $"/api/ticket/{Configuration.SessionId}";
            var request = JsonSerializer.Serialize(new
            {
                PlayerUid = playerUid,
                TicketChange = ticketChange,
                TicketCountNew = ticketCountNew,
                Message = message
            });

            try
            {
                var response = await _client.PostAsync(route, new StringContent(request), cancellationToken);
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
                    var result =
                        await JsonSerializer.DeserializeAsync<PlayerWhitelistingResponse>(
                            await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);

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