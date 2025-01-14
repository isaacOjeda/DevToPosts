var builder = DistributedApplication.CreateBuilder(args);


var ollama =
    builder.AddOllama("ollama")
        .WithDataVolume()
        .WithOpenWebUI();

var llamaModel = ollama.AddModel("llama", "llama3.2");

builder.AddProject<Projects.SemanticKernelLearning01_ApiService>("apiservice")
    .WithReference(llamaModel)
    .WaitFor(llamaModel);

builder.Build().Run();
