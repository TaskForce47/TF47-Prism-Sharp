using System.Text.Json.Serialization;

namespace TF47_Prism_Sharp.Models.Api
{
    public class CreateSessionRequest
    {
        [JsonPropertyName("missionId")]
        public int MissionId { get; set; }
        [JsonPropertyName("missionType")]
        public string MissionType { get; set; }
        [JsonPropertyName("worldName")]
        public string WorldName { get; set; }
    }
}