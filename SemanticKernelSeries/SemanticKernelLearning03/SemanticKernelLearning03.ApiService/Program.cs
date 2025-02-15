using SemanticKernelLearning03.ApiService;
using SemanticKernelLearning03.ApiService.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.AddSemanticKernel();
builder.AddEndpointHandlers();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();

app.MapPost("/api/blog-posts",
    async (CreateBlogPost.Request request, CreateBlogPost.Handler handler, CancellationToken ct) =>
        await handler.Handle(request, ct));

app.MapGet("/api/blog-posts",
    async ([AsParameters] SearchBlogPost.Request request, SearchBlogPost.Handler handler, CancellationToken ct) =>
        await handler.Handle(request, ct));

app.MapGet("/api/blog-posts/qa",
    async ([AsParameters] QuestionsBlogPost.Request request, QuestionsBlogPost.Handler handler, CancellationToken ct) =>
        await handler.Handle(request, ct));

app.Run();