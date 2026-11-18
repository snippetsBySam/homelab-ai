using System.Text.Json;

namespace HomelabAI.Core.OpenAICompatible;

internal static class LlmJsonOptions
{
    internal static JsonSerializerOptions Default { get; } = new(
        JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };
}
