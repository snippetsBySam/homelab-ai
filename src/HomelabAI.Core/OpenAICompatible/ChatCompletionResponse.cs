using System;
using System.Collections.Generic;
using System.Text;

namespace HomelabAI.Core.OpenAICompatible;

internal sealed record ChatCompletionResponse(
    string Model,
    IReadOnlyList<ChatChoice> Choices,
    ChatTokenUsage? Usage);

internal sealed record ChatChoice(
    ChatMessage Message,
    string? FinishReason);

internal sealed record ChatTokenUsage(
    int PromptTokens,
    int CompletionTokens,
    int TotalTokens);
