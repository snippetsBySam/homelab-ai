using HomelabAI.Core.Abstractions;
using HomelabAI.Core.Models;
using System.Net.Http.Json;

namespace HomelabAI.Core.OpenAICompatible;

public sealed class OpenAICompatibleClient(
    HttpClient httpClient) : ILlmClient
{
    public async Task<ChatResult> ChatAsync(string model, string prompt, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(model);
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);

        var request = new ChatCompletionRequest(
            Model: model,
            Messages:
            [
                new ChatMessage(
                    "system",
                    "You are a helpful AI assistant."),
                new ChatMessage("user", prompt)
            ]);

        using var response = await httpClient.PostAsJsonAsync(
            "chat/completions",
            request,
            LlmJsonOptions.Default,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var completion = await response.Content
            .ReadFromJsonAsync<ChatCompletionResponse>(
            LlmJsonOptions.Default,
            cancellationToken);

        if (completion is null)
        {
            throw new InvalidOperationException(
                "The LLM returned an empty response.");
        }

        var choice = completion.Choices?.FirstOrDefault()
            ?? throw new InvalidOperationException(
                "The LLM returned no completion choices.");

        if (choice.Message?.Content is not { } content)
        {
            throw new InvalidOperationException(
                "The LLM returned no message content.");
        }

        return new ChatResult(
            Content: content,
            Model: completion.Model,
            FinishReason: choice.FinishReason,
            Usage: completion.Usage is { } usage
                ? new TokenUsage(
                    usage.PromptTokens,
                    usage.CompletionTokens,
                    usage.TotalTokens)
                : null);
    }
}
