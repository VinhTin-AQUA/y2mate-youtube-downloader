

using System.Text.Json.Serialization;

namespace VideoDownloaderFromY2Mate.Models
{
    public class ConverterResponse
    {
        [JsonPropertyName("filename")]
        public string FileName { get; set; } = string.Empty; 

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty; 

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }
}
