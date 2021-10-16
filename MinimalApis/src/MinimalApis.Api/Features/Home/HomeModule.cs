using Carter;

namespace MinimalApis.Api.Features.Home;
public class HomeModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => "Hola desde Carter");
    }
}
