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
}
