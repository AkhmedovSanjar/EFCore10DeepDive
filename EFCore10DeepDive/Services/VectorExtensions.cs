using Microsoft.Data.SqlTypes;

namespace EFCore10DeepDive.Services;

/// <summary>
/// Extension methods for vector operations
/// </summary>
public static class VectorExtensions
{
    /// <summary>
    /// Normalize vector to unit length (magnitude = 1)
    /// </summary>
    public static float[] Normalize(this float[] vector)
    {
        var magnitude = vector.CalculateMagnitude();
        
        if (magnitude > 0)
        {
            for (int i = 0; i < vector.Length; i++)
                vector[i] /= (float)magnitude;
        }

        return vector;
    }

    /// <summary>
    /// Calculate vector magnitude (Euclidean norm)
    /// </summary>
    public static double CalculateMagnitude(this float[] vector)
    {
        return Math.Sqrt(vector.Sum(x => (double)x * x));
    }

    /// <summary>
    /// Pad or truncate vector to target dimension
    /// </summary>
    public static SqlVector<float> ToSqlVector(this float[] vector, int targetDimension)
    {
        if (vector.Length == targetDimension)
            return new SqlVector<float>(vector.Normalize());

        var resized = new float[targetDimension];
        var copyLength = Math.Min(vector.Length, targetDimension);
        Array.Copy(vector, resized, copyLength);

        return new SqlVector<float>(resized.Normalize());
    }

    /// <summary>
    /// Calculate cosine similarity between two vectors
    /// Returns value between -1 (opposite) and 1 (identical)
    /// </summary>
    public static double CosineSimilarity(this float[] vectorA, float[] vectorB)
    {
        if (vectorA.Length != vectorB.Length)
            throw new ArgumentException("Vectors must have the same dimension");

        var dotProduct = vectorA.DotProduct(vectorB);
        var magnitudeA = vectorA.CalculateMagnitude();
        var magnitudeB = vectorB.CalculateMagnitude();

        return dotProduct / (magnitudeA * magnitudeB);
    }

    /// <summary>
    /// Calculate dot product of two vectors
    /// </summary>
    public static double DotProduct(this float[] vectorA, float[] vectorB)
    {
        if (vectorA.Length != vectorB.Length)
            throw new ArgumentException("Vectors must have the same dimension");

        return vectorA.Zip(vectorB, (a, b) => (double)a * b).Sum();
    }

    /// <summary>
    /// Calculate Euclidean distance between two vectors
    /// </summary>
    public static double EuclideanDistance(this float[] vectorA, float[] vectorB)
    {
        if (vectorA.Length != vectorB.Length)
            throw new ArgumentException("Vectors must have the same dimension");

        var sumOfSquares = vectorA
            .Zip(vectorB, (a, b) => Math.Pow(a - b, 2))
            .Sum();

        return Math.Sqrt(sumOfSquares);
    }
}
