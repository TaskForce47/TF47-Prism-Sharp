using System.Text.Json.Serialization;

namespace TF47_Prism_Sharp.Models.Api
{
    public class CreatePlayerRequest
    {
        [JsonPropertyName("playerUid")]
        public string PlayerUid { get; set; }
        [JsonPropertyName("playerName")]
        public string PlayerName { get; set; }
    }
}