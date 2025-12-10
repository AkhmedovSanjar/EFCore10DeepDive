using Microsoft.Data.SqlTypes;

namespace EFCore10DeepDive.Services;

/// <summary>
/// Strategy interface for embedding providers
/// </summary>
public interface IEmbeddingProvider
{
    /// <summary>
    /// Generate embedding vector from text
    /// </summary>
    Task<SqlVector<float>> GenerateAsync(string text);

    /// <summary>
    /// Check if provider is available and configured
    /// </summary>
    bool IsAvailable { get; }
}
