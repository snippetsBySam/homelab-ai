using HomelabAI.Core.Models;

namespace HomelabAI.Core.Abstractions;

public interface ILlmClient
{
    Task<ChatResult> ChatAsync(
        string model,
        string prompt,
        CancellationToken cancellationToken = default);
}
