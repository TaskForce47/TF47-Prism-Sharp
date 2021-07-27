using System;
using System.Text.Json.Serialization;

namespace TF47_Prism_Sharp.Models.Api
{
    public class SessionResponse
    {
        [JsonPropertyName("sessionId")]
        public int SessionId { get; set; }

        [JsonPropertyName("missionId")]
        public int MissionId { get; set; }

        [JsonPropertyName("missionName")]
        public string MissionName { get; set; }

        [JsonPropertyName("worldName")]
        public string WorldName { get; set; }

        [JsonPropertyName("timeSessionCreated")]
        public DateTime TimeSessionCreated { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        [JsonPropertyName("timeSessionEnded")]
        public DateTime? TimeSessionEnded { get; set; }
    }
}