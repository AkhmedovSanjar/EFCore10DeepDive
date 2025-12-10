using Microsoft.Data.SqlTypes;
using OpenAI.Embeddings;

namespace EFCore10DeepDive.Services;

/// <summary>
/// Embedding generation service
/// </summary>
public class EmbeddingService
{
    private readonly EmbeddingClient? _embeddingGenerator;
    public EmbeddingService() => _embeddingGenerator = AIConfiguration.CreateEmbeddingGenerator();

    /// <summary>
    /// Generate vector embedding from text
    /// </summary>
    public SqlVector<float> GenerateEmbedding(string text)
    {
        if (_embeddingGenerator != null)
        {
            try
            {
                var embeddingRes = _embeddingGenerator.GenerateEmbedding(text);
                var embedding = embeddingRes.Value.ToFloats();
                if (embedding.Length > 0)
                    return new SqlVector<float>(embedding);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AI Warning] Failed to generate AI embedding: {ex.Message}");
            }
        }

        return GenerateDemoEmbedding(text);
    }

    /// <summary>
    /// Demo embedding generation
    /// </summary>
    private SqlVector<float> GenerateDemoEmbedding(string text)
    {
        const int VectorDimensions = 1536;
        var random = new Random(text.GetHashCode());

        var vector = new float[VectorDimensions];
        for (int i = 0; i < VectorDimensions; i++)
            vector[i] = (float)(random.NextDouble() * 2 - 1);

        var magnitude = Math.Sqrt(vector.Sum(x => x * x));
        for (int i = 0; i < VectorDimensions; i++)
            vector[i] /= (float)magnitude;

        return new SqlVector<float>(vector);
    }

    /// <summary>
    /// Check if real AI is configured and available
    /// </summary>
    public bool IsAIAvailable => _embeddingGenerator != null;
}
