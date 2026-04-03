using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace SDWallpaperEngine.ComfyUi
{
    public sealed record ComfyUiPromptSubmissionResult
    {
        [JsonPropertyName("prompt_id")]
        public string? PromptId { get; init; }

        public int? Number { get; init; }

        public JsonNode? Error { get; init; }

        [JsonPropertyName("node_errors")]
        public JsonNode? NodeErrors { get; init; }
    }

    public sealed record ComfyUiSystemStats
    {
        [JsonPropertyName("python_version")]
        public string? PythonVersion { get; init; }

        [JsonPropertyName("devices")]
        public JsonNode? Devices { get; init; }
    }

    public sealed record ComfyUiQueueState
    {
        [JsonPropertyName("queue_running")]
        public JsonNode? Running { get; init; }

        [JsonPropertyName("queue_pending")]
        public JsonNode? Pending { get; init; }
    }

    public sealed record ComfyUiHistoryEntry
    {
        [JsonPropertyName("prompt_id")]
        public string? PromptId { get; init; }

        public JsonNode? Prompt { get; init; }

        public JsonNode? Outputs { get; init; }

        public JsonNode? Status { get; init; }
    }

    public sealed record ComfyUiObjectInfoResponse
    {
        [JsonPropertyName("class_type")]
        public string? ClassType { get; init; }

        [JsonPropertyName("display_name")]
        public string? DisplayName { get; init; }

        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("input")]
        public JsonNode? Input { get; init; }

        [JsonPropertyName("output")]
        public JsonNode? Output { get; init; }
    }

    public sealed record ComfyUiUserDataFile
    {
        public string? Name { get; init; }

        public string? Path { get; init; }

        public bool? IsDirectory { get; init; }

        public long? Size { get; init; }

        public DateTimeOffset? ModifiedAt { get; init; }

        [JsonPropertyName("children")]
        public JsonNode? Children { get; init; }
    }

    public sealed record ComfyUiWebSocketMessage
    {
        [JsonPropertyName("type")]
        public string? Type { get; init; }

        [JsonPropertyName("data")]
        public JsonNode? Data { get; init; }

        public static ComfyUiWebSocketMessage Parse(string json)
        {
            return JsonSerializer.Deserialize<ComfyUiWebSocketMessage>(json, JsonOptions) ?? new ComfyUiWebSocketMessage();
        }

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };
    }
}
