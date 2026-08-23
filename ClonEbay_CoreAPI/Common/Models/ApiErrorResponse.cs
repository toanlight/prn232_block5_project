using System.Text.Json.Serialization;

namespace ClonEbay_CoreAPI.Common.Models
{
    /// <summary>
    /// Model chuẩn cho Error Response trả về cho Client
    /// </summary>
    public class ApiErrorResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; } = false;

        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("errors")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Errors { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("traceId")]
        public string TraceId { get; set; } = string.Empty;
    }
}
