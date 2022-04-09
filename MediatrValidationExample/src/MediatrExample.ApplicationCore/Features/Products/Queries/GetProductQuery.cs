using AutoMapper;
using MediatR;
using MediatrExample.ApplicationCore.Common.Exceptions;
using MediatrExample.ApplicationCore.Common.Helpers;
using MediatrExample.ApplicationCore.Domain;
using MediatrExample.ApplicationCore.Infrastructure.Persistence;


namespace MediatrExample.ApplicationCore.Features.Products.Queries;

public class GetProductQuery : IRequest<GetProductQueryResponse>
{
    public string ProductId { get; set; }
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
        var product = await _context.Products.FindAsync(request.ProductId.FromHashId());

        if (product is null)
        {
            throw new NotFoundException(nameof(Product), request.ProductId);
        }

        return _mapper.Map<GetProductQueryResponse>(product);
    }
}

public class GetProductQueryResponse
{
    public string ProductId { get; set; } = default!;
    public string Description { get; set; } = default!;
    public double Price { get; set; }
}

public class GetProductQueryProfile : Profile
{
    public GetProductQueryProfile() =>
        CreateMap<Product, GetProductQueryResponse>()
            .ForMember(dest =>
                dest.ProductId,
                opt => opt.MapFrom(mf => mf.ProductId.ToHashId()));

}