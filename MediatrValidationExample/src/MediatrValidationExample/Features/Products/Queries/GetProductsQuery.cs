using MediatR;
using MediatrValidationExample.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MediatrValidationExample.Features.Products.Queries;

public class GetProductsQuery : IRequest<List<GetProductsResponse>>
{

}

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<GetProductsResponse>>
{
    private readonly MyAppDbContext _context;

    public GetProductsQueryHandler(MyAppDbContext context)
    {
        _context = context;
    }

    public Task<List<GetProductsResponse>> Handle(GetProductsQuery request, CancellationToken cancellationToken) =>
        _context.Products
            .AsNoTracking()
            .Select(s => new GetProductsResponse
            {
                ProductId = s.ProductId,
                Description = s.Description,
                Price = s.Price
            })
            .ToListAsync();
}

public class GetProductsResponse
{
    public int ProductId { get; set; }
    public string Description { get; set; } = default!;
    public double Price { get; set; }
}
