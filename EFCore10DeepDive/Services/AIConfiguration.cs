using Azure;
using OpenAI;
using OpenAI.Embeddings;

namespace EFCore10DeepDive.Services;

/// <summary>
/// Configuration helper for AI embedding providers
/// </summary>
public static class AIConfiguration
{
    public static EmbeddingClient? CreateEmbeddingGenerator()
    {
        var azureKey = "";
        if (!string.IsNullOrEmpty(azureKey))
        {
            var endpoint = new Uri("");
            var credential = new AzureKeyCredential(azureKey);
            var deploymentName = "text-embedding-3-small";

            var openAIOptions = new OpenAIClientOptions()
            {
                Endpoint = endpoint
            };
            var azureOpenAIClient = new OpenAIClient(credential, openAIOptions);
            var embeddingClient = azureOpenAIClient.GetEmbeddingClient(deploymentName);
            return embeddingClient;
        }

        return null;
    }

    /// <summary>
    /// Get Google Gemini API key from configuration
    /// Set your API key here or use environment variable GEMINI_API_KEY
    /// </summary>
    public static string? GetGeminiApiKey()
    {
        // Option 1: Hard-coded API key (for development only)
        var hardcodedKey = "AIzaSyDI6vgn0oypP9_iqNSq0SSZ57s9KL4CWhs";
        if (!string.IsNullOrEmpty(hardcodedKey))
            return hardcodedKey;

        // Option 2: Environment variable (recommended for production)
        var envKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        if (!string.IsNullOrEmpty(envKey))
            return envKey;

        return null;
    }
}
