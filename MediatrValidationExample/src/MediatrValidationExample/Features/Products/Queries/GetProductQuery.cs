using AutoMapper;
using MediatR;
using MediatrValidationExample.Domain;
using MediatrValidationExample.Exceptions;
using MediatrValidationExample.Infrastructure.Persistence;

namespace MediatrValidationExample.Features.Products.Queries;

public class GetProductQuery : IRequest<GetProductQueryResponse>
{
    public int ProductId { get; set; }
}

public class GetProductQueryHandler : IRequestHandler<GetProductQuery, GetProductQueryResponse>
{
    private readonly MyAppDbContext _context;
    private readonly IMapper _mapper;

    public GetProductQueryHandler(MyAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<GetProductQueryResponse> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync(request.ProductId);

        if (product is null)
        {
            throw new NotFoundException(nameof(Product), request.ProductId);
        }

        return _mapper.Map<GetProductQueryResponse>(product);
    }
}

public class GetProductQueryResponse
{
    public int ProductId { get; set; }
    public string Description { get; set; } = default!;
    public double Price { get; set; }
}

public class GetProductQueryProfile : Profile
{
    public GetProductQueryProfile() =>
        CreateMap<Product, GetProductQueryResponse>();

}