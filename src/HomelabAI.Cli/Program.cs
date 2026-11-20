using HomelabAI.Cli.Configuration;
using HomelabAI.Core.Abstractions;
using HomelabAI.Core.OpenAICompatible;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Diagnostics;

var builder = Host.CreateApplicationBuilder(
    new HostApplicationBuilderSettings
    {
        Args = args,
        ContentRootPath = AppContext.BaseDirectory
    });

var services = builder.Services;
var configs = builder.Configuration;

services
    .AddOptions<LlmOptions>()
    .Bind(configs.GetSection(
        LlmOptions.SectionName))
    .Validate(
        options => options.BaseUrl is
        {
            IsAbsoluteUri: true,
            Scheme: "http" or "https"
        },
        "A valid HTTP(S) base URL is required.")
    .Validate(
        options => options.BaseUrl?.AbsolutePath.EndsWith(
            "/v1/", StringComparison.Ordinal) == true,
        "Llm:BaseUrl must end with /v1/.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Model),
        "Llm:Model must be configured.")
    .Validate(
        options => options.Timeout > TimeSpan.Zero,
        "Llm:Timeout must be positive.")
    .ValidateOnStart();

services.AddHttpClient<
    ILlmClient,
    OpenAICompatibleClient>((provider, client) =>
{
    var options = provider
        .GetRequiredService<IOptions<LlmOptions>>()
        .Value;

    client.BaseAddress = options.BaseUrl;
    client.Timeout = options.Timeout;
});

using var host = builder.Build();

await host.StartAsync();

try
{
    var client = host.Services
        .GetRequiredService<ILlmClient>();

    var options = host.Services
        .GetRequiredService<IOptions<LlmOptions>>()
        .Value;

    Console.Write("Prompt: ");
    var prompt = Console.ReadLine();

    if (!string.IsNullOrWhiteSpace(prompt))
    {
        using var cts = new CancellationTokenSource();

        ConsoleCancelEventHandler handler = (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        Console.CancelKeyPress += handler;

        try
        {
            var stopwatch = Stopwatch.StartNew();

            var result = await client.ChatAsync(
                options.Model,
                prompt,
                cts.Token);

            stopwatch.Stop();

            Console.WriteLine($"\n{result.Content}");
            Console.WriteLine(
                $"\nDuration: {stopwatch.Elapsed.TotalSeconds:F2}s");
            Console.WriteLine($"Model: {result.Model}");
            Console.WriteLine($"Finish reason: {result.FinishReason}");

            if (result.Usage is { } usage)
            {
                Console.WriteLine(
                    $"Prompt tokens: {usage.PromptTokens}");
                Console.WriteLine(
                    $"Completion tokens: {usage.CompletionTokens}");
            }
        }
        catch (OperationCanceledException)
            when (cts.IsCancellationRequested)
        {
            Console.WriteLine("Request cancelled");
        }
        finally
        {
            Console.CancelKeyPress -= handler;
        }
    }
}
finally
{

    await host.StopAsync();
}
