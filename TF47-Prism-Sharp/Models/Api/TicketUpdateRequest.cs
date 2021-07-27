using System.Text.Json.Serialization;

namespace TF47_Prism_Sharp.Models.Api
{
    public class TicketUpdateRequest
    {
        [JsonPropertyName("playerUid")]
        public string PlayerUid { get; set; }
        [JsonPropertyName("ticketChange")]
        public int TicketChange { get; set; }
        [JsonPropertyName("ticketCountNew")]
        public int TicketCountNew { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}