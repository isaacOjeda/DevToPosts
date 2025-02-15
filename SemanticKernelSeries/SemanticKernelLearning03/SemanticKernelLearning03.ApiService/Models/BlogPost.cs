using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;

namespace SemanticKernelLearning03.ApiService.Models;

#pragma warning disable SKEXP0001

public class BlogPost
{
    public const string VectorName = "blogposts";

    [VectorStoreRecordKey]
    [TextSearchResultLink]
    public Guid BlogPostId { get; set; }

    [VectorStoreRecordData(IsFilterable = true)]
    [TextSearchResultName]
    public string Title { get; set; }

    [VectorStoreRecordData(IsFullTextSearchable = true)]
    [TextSearchResultValue]
    public string Description { get; set; }

    [VectorStoreRecordVector(768, DistanceFunction.DotProductSimilarity, IndexKind.Hnsw)]
    public ReadOnlyMemory<float>? DescriptionEmbedding { get; set; }

    [VectorStoreRecordData(IsFilterable = true)]
    public string[] Tags { get; set; }
}

#pragma warning restore SKEXP0001