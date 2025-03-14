using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using SemanticKernelLearning03.ApiService.Models;

namespace SemanticKernelLearning03.ApiService.Endpoints;

#pragma warning disable SKEXP0070
#pragma warning disable SKEXP0001

public static class SearchBlogPost
{
    public record Request(string Query);

    public record Response(Guid BlogPostId, string Title, string Description, double Relevance);

    public class Handler(
        IVectorStore vectorStore,
        ITextEmbeddingGenerationService embeddingGenerator)
    {
        public async Task<List<Response>> Handle(Request request, CancellationToken ct)
        {
            // Generate embeddings for the query
            ReadOnlyMemory<float> queryEmbedding = await embeddingGenerator.GenerateEmbeddingAsync(request.Query, cancellationToken: ct);

            var blogposts = vectorStore.GetCollection<Guid, BlogPost>(BlogPost.VectorName);

            // Search with vector store directly
            var searchResults = await blogposts.VectorizedSearchAsync(queryEmbedding, new VectorSearchOptions
            {
                Top = 5,

            });

            List<Response> responses = new();
            await foreach (var result in searchResults.Results)
            {
                if (result.Score < 0.5)
                {
                    continue;
                }

                responses.Add(new Response(
                    result.Record.BlogPostId,
                    result.Record.Title,
                    result.Record.Description,
                    result.Score ?? 0));
            }

            return responses;
        }
    }
}

#pragma warning restore SKEXP0001
#pragma warning restore SKEXP0070