using System;
using System.Text.Json.Serialization;

namespace TF47_Prism_Sharp.Models.Api
{
    public class PlayerResponse
    {
        [JsonPropertyName("playerUid")]
        public string PlayerUid { get; set; }

        [JsonPropertyName("playerName")]
        public string PlayerName { get; set; }

        [JsonPropertyName("timeFirstVisit")]
        public DateTime TimeFirstVisit { get; set; }

        [JsonPropertyName("timeLastVisit")]
        public DateTime TimeLastVisit { get; set; }

        [JsonPropertyName("numberConnections")]
        public int NumberConnections { get; set; }
    }
}