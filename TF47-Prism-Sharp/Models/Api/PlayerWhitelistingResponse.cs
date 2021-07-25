using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TF47_Prism_Sharp.Models.Api
{
    public class PlayerWhitelistingResponse
    {
        [JsonPropertyName("playerUid")]
        public string PlayerUid { get; set; }

        [JsonPropertyName("playerName")]
        public string PlayerName { get; set; }

        [JsonPropertyName("whitelistings")]
        public List<Whitelisting> Whitelistings { get; set; }
    }
}