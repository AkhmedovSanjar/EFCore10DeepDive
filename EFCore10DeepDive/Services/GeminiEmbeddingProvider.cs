using Microsoft.Data.SqlTypes;
using System.Net.Http.Json;
using System.Text.Json;

namespace EFCore10DeepDive.Services;

/// <summary>
/// Google Gemini API embedding provider
/// </summary>
public class GeminiEmbeddingProvider : IEmbeddingProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly GeminiConfig _config;

    public GeminiEmbeddingProvider(string apiKey) : this(apiKey, new HttpClient(), GeminiConfig.Default)
    { }

    public GeminiEmbeddingProvider(string apiKey, HttpClient httpClient, GeminiConfig config)
    {
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public bool IsAvailable => !string.IsNullOrEmpty(_apiKey);

    public async Task<SqlVector<float>> GenerateAsync(string text)
    {
        try
        {
            var embedding = await CallGeminiApiAsync(text);
            return embedding.ToSqlVector(_config.TargetDimension);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AI Warning] Gemini API failed: {ex.Message}");
            throw new EmbeddingGenerationException("Failed to generate Gemini embedding", ex);
        }
    }

    private async Task<float[]> CallGeminiApiAsync(string text)
    {
        var request = CreateRequest(text);
        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Gemini API error: {response.StatusCode} - {error}");
        }

        return await ParseResponseAsync(response);
    }

    private HttpRequestMessage CreateRequest(string text)
    {
        var requestBody = new
        {
            content = new
            {
                parts = new[] { new { text } }
            },
            taskType = _config.TaskType,
            outputDimensionality = _config.OutputDimension
        };

        var request = new HttpRequestMessage(HttpMethod.Post, _config.GetApiUrl(_apiKey));
        request.Content = JsonContent.Create(requestBody);
        request.Headers.Add("Accept", "application/json");

        return request;
    }

    private static async Task<float[]> ParseResponseAsync(HttpResponseMessage response)
    {
        var jsonResponse = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonResponse);
        return doc.RootElement
            .GetProperty("embedding")
            .GetProperty("values")
            .EnumerateArray()
            .Select(v => (float)v.GetDouble())
            .ToArray()
            .Normalize();
    }
}

/// <summary>
/// Configuration for Gemini API
/// </summary>
public record GeminiConfig(
    string Model = "gemini-embedding-001",
    string TaskType = "RETRIEVAL_DOCUMENT",
    int OutputDimension = 768,
    int TargetDimension = 1536)
{
    public static GeminiConfig Default => new();
    public string GetApiUrl(string apiKey)
        => $"https://generativelanguage.googleapis.com/v1beta/models/{Model}:embedContent?key={apiKey}";
}

/// <summary>
/// Custom exception for embedding generation errors
/// </summary>
public class EmbeddingGenerationException : Exception
{
    public EmbeddingGenerationException(string message, Exception innerException)
        : base(message, innerException) { }
}
