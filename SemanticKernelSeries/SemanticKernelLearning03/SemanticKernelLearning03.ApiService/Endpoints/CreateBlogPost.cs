using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Embeddings;
using SemanticKernelLearning03.ApiService.Models;

namespace SemanticKernelLearning03.ApiService.Endpoints;

#pragma warning disable SKEXP0070
#pragma warning disable SKEXP0001

public static class CreateBlogPost
{
    public record Request(string Title, string Description, string[] Tags);

    public record Response(Guid BlogPostId);

    public class Handler
    {
        private readonly ITextEmbeddingGenerationService _embeddingService;
        private readonly IVectorStore _vectorStore;

        public Handler(ITextEmbeddingGenerationService embeddingService, IVectorStore vectorStore)
        {
            _embeddingService = embeddingService;
            _vectorStore = vectorStore;
        }

        public async Task<Response> Handle(Request request, CancellationToken ct)
        {
            var blogposts = _vectorStore.GetCollection<Guid, BlogPost>(BlogPost.VectorName);
            await blogposts.CreateCollectionIfNotExistsAsync(ct);
            var embeddingContents =
                await _embeddingService.GenerateEmbeddingAsync(request.Description, cancellationToken: ct);
            var newBlogPost = new BlogPost
            {
                BlogPostId = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                DescriptionEmbedding = embeddingContents,
                Tags = request.Tags
            };
            await blogposts.UpsertAsync(newBlogPost, cancellationToken: ct);
            return new Response(newBlogPost.BlogPostId);
        }
    }
}

#pragma warning restore SKEXP0001
#pragma warning restore SKEXP0070