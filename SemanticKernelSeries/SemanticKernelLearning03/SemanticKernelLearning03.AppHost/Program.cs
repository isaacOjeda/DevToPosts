var builder = DistributedApplication.CreateBuilder(args);


var qdrant = builder.AddQdrant("qdrant")
    .WithLifetime(ContainerLifetime.Persistent);

var ollama =
    builder.AddOllama("ollama")
        .WithDataVolume()
        .WithOpenWebUI();

var llamaModel = ollama.AddModel("llama", "llama3.2");
var embedding = ollama.AddModel("embed-text", "nomic-embed-text");

var apiService = builder.AddProject<Projects.SemanticKernelLearning03_ApiService>("apiservice")
    .WithReference(llamaModel)
    .WithReference(embedding)
    .WithReference(qdrant);

builder.AddProject<Projects.SemanticKernelLearning03_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
