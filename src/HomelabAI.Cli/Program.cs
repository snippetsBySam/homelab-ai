using HomelabAI.Core.Abstractions;
using HomelabAI.Core.OpenAICompatible;
using System.Diagnostics;

const string endpoint = "http://evox2server:8080/v1/";
const string model = "qwen3.8-27b-q8";

using var httpClient = new HttpClient
{
    BaseAddress = new Uri(endpoint),
    Timeout = TimeSpan.FromSeconds(10)
};

ILlmClient llmClient = new OpenAICompatibleClient(httpClient);

Console.Write("Prompt: ");

var prompt = Console.ReadLine();

if (string.IsNullOrWhiteSpace(prompt))
{
    Console.WriteLine("No Prompt Provided.");
    return;
}

using var cts = new CancellationTokenSource();

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

var stopwatch = Stopwatch.StartNew();

try
{
    Console.WriteLine("\nGenerating...\n");

    var result = await llmClient.ChatAsync(
        model, prompt, cts.Token);

    stopwatch.Stop();

    Console.WriteLine(result.Content);

    Console.WriteLine("\n--- Metrics ---");
    Console.WriteLine($"Model: {result.Model}");
    Console.WriteLine($"Duration: {stopwatch.Elapsed.TotalSeconds:F2}s");
    Console.WriteLine($"Finish reason: {result.FinishReason}");

    if (result.Usage is { } usage)
    {
        Console.WriteLine($"Prompt tokens: {usage.PromptTokens}");
        Console.WriteLine(
            $"Completion tokens: {usage.CompletionTokens}");
        Console.WriteLine($"Total tokens: {usage.TotalTokens}");
    }
}
catch (OperationCanceledException) when (cts.IsCancellationRequested)
{
    Console.WriteLine("Request cancelled.");
}
catch (HttpRequestException ex)
{
    Console.Error.WriteLine(
        $"HTTP request failed: {ex.Message}");
}
