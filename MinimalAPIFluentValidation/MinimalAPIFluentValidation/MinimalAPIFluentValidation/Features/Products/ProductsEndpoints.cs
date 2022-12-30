using MinimalAPIFluentValidation.Common;

namespace MinimalAPIFluentValidation.Features.Products;


public static class ProductsEndpoints
{
    public static RouteGroupBuilder MapProducts(this WebApplication app)
    {
        var group = app.MapGroup("api/products");

        group.MapPost("/", CreateProductHandler.Handler)
            .WithName("CreateProduct");

        // other endpoints here...

        group.WithTags(new string[] { "Products" });
        group.WithOpenApi();


        group.AddEndpointFilterFactory(ValidationFilter.ValidationFilterFactory);

        return group;
    }
}