using Carter;
using DomainEventsExample.Domain.Entities;
using DomainEventsExample.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DomainEventsExample.Features.Products.Queries;

public class GetProducts : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/products", (IMediator mediator) =>
        {
            return mediator.Send(new GetProductsQuery());
        })
        .WithName(nameof(GetProducts))
        .WithTags(nameof(Product));
    }

    public class GetProductsQuery : IRequest<List<GetProductsResponse>>
    {

    }

    public class GetProductsHandler : IRequestHandler<GetProductsQuery, List<GetProductsResponse>>
    {
        private readonly MyDbContext _context;

        public GetProductsHandler(MyDbContext context)
        {
            _context = context;
        }

        public Task<List<GetProductsResponse>> Handle(GetProductsQuery request, CancellationToken cancellationToken) =>
            _context.Products
                .Select(s => new GetProductsResponse
                (
                    s.ProductId, s.Name, s.Description, s.Price
                ))
                .ToListAsync();
    }


    public record GetProductsResponse(int ProductId, string Name, string Description, double Price);
}
