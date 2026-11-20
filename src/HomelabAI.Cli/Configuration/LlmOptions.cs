namespace HomelabAI.Cli.Configuration;

public sealed class LlmOptions
{
    public const string SectionName = "Llm";

    public required Uri BaseUrl { get; init; }

    public required string Model { get; init; }

    public TimeSpan Timeout { get; init; } =
        TimeSpan.FromMinutes(10);
}
