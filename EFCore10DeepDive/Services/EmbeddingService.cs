using Microsoft.Data.SqlTypes;

namespace EFCore10DeepDive.Services;

/// <summary>
/// Embedding generation service using Google Gemini API if configured, otherwise falls back to demo provider
/// Implements Strategy pattern for different embedding providers
/// </summary>
public class EmbeddingService
{
    private readonly IEmbeddingProvider _provider;
    public EmbeddingService() => _provider = CreateProvider();

    /// <summary>
    /// Generate vector embedding from text
    /// </summary>
    public async Task<SqlVector<float>> GenerateEmbeddingAsync(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text, nameof(text));
        return await _provider.GenerateAsync(text);
    }

    /// <summary>
    /// Check if real AI is configured and available
    /// </summary>
    public bool IsAIAvailable => _provider.IsAvailable;

    private static IEmbeddingProvider CreateProvider()
    {
        var apiKey = AIConfiguration.GetGeminiApiKey();
        return string.IsNullOrEmpty(apiKey)
            ? new DemoEmbeddingProvider()
            : new GeminiEmbeddingProvider(apiKey);
    }
}
