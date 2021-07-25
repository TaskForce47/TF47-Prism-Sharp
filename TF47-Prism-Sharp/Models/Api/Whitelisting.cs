using System.Text.Json.Serialization;

namespace TF47_Prism_Sharp.Models.Api
{
    public class Whitelisting
    {
        [JsonPropertyName("whitelistId")]
        public int WhitelistId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}