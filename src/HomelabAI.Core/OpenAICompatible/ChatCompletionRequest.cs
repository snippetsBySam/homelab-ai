namespace HomelabAI.Core.OpenAICompatible;

internal sealed record ChatCompletionRequest(
    string Model,
    IReadOnlyList<ChatMessage> Messages,
    bool Stream = false);

internal sealed record ChatMessage(
    string Role,
    string Content);
