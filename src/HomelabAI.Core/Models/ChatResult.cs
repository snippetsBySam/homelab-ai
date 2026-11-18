namespace HomelabAI.Core.Models;

public sealed record ChatResult(
    string Content,
    string Model,
    string? FinishReason,
    TokenUsage? Usage);

public sealed record TokenUsage(
    int PromptTokens,
    int CompletionTokens,
    int TotalTokens);
