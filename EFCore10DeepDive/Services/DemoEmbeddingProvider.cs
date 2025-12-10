using Microsoft.Data.SqlTypes;

namespace EFCore10DeepDive.Services;

/// <summary>
/// Demo/Fallback embedding provider using deterministic hash-based generation
/// Used when no API key is configured or API fails
/// </summary>
public class DemoEmbeddingProvider : IEmbeddingProvider
{
    private readonly int _dimensions;
    public DemoEmbeddingProvider(int dimensions = 1536) => _dimensions = dimensions;

    public bool IsAvailable => true;

    public Task<SqlVector<float>> GenerateAsync(string text)
    {
        var vector = GenerateDeterministicVector(text);
        return Task.FromResult(new SqlVector<float>(vector));
    }

    private float[] GenerateDeterministicVector(string text)
    {
        var random = new Random(text.GetHashCode());
        var vector = new float[_dimensions];
        for (int i = 0; i < _dimensions; i++)
            vector[i] = (float)(random.NextDouble() * 2 - 1);
        return vector.Normalize();
    }
}
