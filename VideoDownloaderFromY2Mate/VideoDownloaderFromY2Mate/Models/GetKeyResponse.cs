

using System.Text.Json.Serialization;

namespace VideoDownloaderFromY2Mate.Models
{
    public class GetKeyResponse
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;
    }
}
