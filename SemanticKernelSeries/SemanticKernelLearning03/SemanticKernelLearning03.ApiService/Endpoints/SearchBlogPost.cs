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

    public record Response(Guid BlogPostId, string Title, string Description);

    public class Handler([FromKeyedServices(BlogPost.VectorName)] ITextSearch textSearch)
    {
        public async Task<List<Response>> Handle(Request request, CancellationToken ct)
        {
            KernelSearchResults<TextSearchResult> textResults =
                await textSearch.GetTextSearchResultsAsync(request.Query, new()
                {
                    Top = 2,
                    Skip = 0,
                }, ct);

            List<Response> responses = new();
            await foreach (TextSearchResult result in textResults.Results.WithCancellation(ct))
            {
                responses.Add(new(
                    Guid.Parse(result.Link),
                    result.Name,
                    result.Value));
            }

            return responses;
        }
    }
}

#pragma warning restore SKEXP0001
#pragma warning restore SKEXP0070