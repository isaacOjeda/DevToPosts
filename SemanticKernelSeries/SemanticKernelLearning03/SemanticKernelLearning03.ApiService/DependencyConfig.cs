using Microsoft.SemanticKernel;
using System.Data.Common;
using Microsoft.Extensions.VectorData;
using SemanticKernelLearning03.ApiService.Endpoints;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using SemanticKernelLearning03.ApiService.Models;

namespace SemanticKernelLearning03.ApiService;

#pragma warning disable SKEXP0070
#pragma warning disable SKEXP0001

public static class DependencyConfig
{
    public static void AddSemanticKernel(this WebApplicationBuilder builder)
    {
        var (endpoint, completionModel) =
            GetModelDetailsFromConnectionString(builder.Configuration.GetConnectionString("llama")!);
        var (_, embeddingModel) =
            GetModelDetailsFromConnectionString(builder.Configuration.GetConnectionString("embed-text")!);
        var (host, port, apiKey) =
            GetQdrantDetailsFromConnectionString(builder.Configuration.GetConnectionString("qdrant")!);

        builder.Services.AddKernel()
            .AddOllamaChatCompletion(completionModel, endpoint)
            .AddOllamaTextEmbeddingGeneration(embeddingModel, endpoint)
            .AddQdrantVectorStore(host: host, port: port, apiKey: apiKey);

        builder.Services.AddKeyedTransient<ITextSearch>(BlogPost.VectorName, (sp, _) =>
        {
            var vectorStore = sp.GetRequiredService<IVectorStore>();
            var embeddingGenerationService = sp.GetRequiredService<ITextEmbeddingGenerationService>();

            var blogposts = vectorStore.GetCollection<Guid, BlogPost>(BlogPost.VectorName);

            blogposts.CreateCollectionIfNotExistsAsync().ConfigureAwait(true);

            var textSearch = new VectorStoreTextSearch<BlogPost>(blogposts, embeddingGenerationService);

            return textSearch;
        });
    }

    public static void AddEndpointHandlers(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<CreateBlogPost.Handler>();
        builder.Services.AddTransient<SearchBlogPost.Handler>();
        builder.Services.AddTransient<QuestionsBlogPost.Handler>();
    }

    private static (Uri endpoint, string modelId) GetModelDetailsFromConnectionString(string connectionString)
    {
        var connectionBuilder = new DbConnectionStringBuilder
        {
            ConnectionString = connectionString
        };

        Uri endpoint = new Uri(connectionBuilder["Endpoint"].ToString());
        string modelId = connectionBuilder["Model"].ToString();

        return (endpoint, modelId);
    }

    private static (string Host, int Port, string ApiKey) GetQdrantDetailsFromConnectionString(string connectionString)
    {
        var connectionBuilder = new DbConnectionStringBuilder
        {
            ConnectionString = connectionString
        };
        var endpoint = connectionBuilder["Endpoint"].ToString();

        var host = new Uri(endpoint).Host;
        var port = new Uri(endpoint).Port;
        var apiKey = connectionBuilder["Key"].ToString();

        return (host, port, apiKey);
    }
}


#pragma warning restore SKEXP0001
#pragma warning restore SKEXP0070