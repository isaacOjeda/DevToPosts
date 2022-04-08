using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using MediatrExample.ApplicationCore.Common.Helpers;
using MediatrExample.ApplicationCore.Domain;
using MediatrExample.ApplicationCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MediatrExample.ApplicationCore.Features.Products.Queries;

public class GetProductsQuery : IRequest<List<GetProductsQueryResponse>>
{

}

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<GetProductsQueryResponse>>
{
    private readonly MyAppDbContext _context;
    private readonly IMapper _mapper;

    public GetProductsQueryHandler(MyAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public Task<List<GetProductsQueryResponse>> Handle(GetProductsQuery request, CancellationToken cancellationToken) =>
        _context.Products
            .AsNoTracking()
            .ProjectTo<GetProductsQueryResponse>(_mapper.ConfigurationProvider)
            .ToListAsync();
}

public class GetProductsQueryResponse
{
    public string ProductId { get; set; } = default!;
    public string Description { get; set; } = default!;
    public double Price { get; set; }
    public string ListDescription { get; set; } = default!;
}

public class GetProductsQueryProfile : Profile
{
    public GetProductsQueryProfile() =>
        CreateMap<Product, GetProductsQueryResponse>()
            .ForMember(dest =>
                dest.ListDescription,
                opt => opt.MapFrom(mf => $"{mf.Description} - {mf.Price:c}"))
            .ForMember(dest =>
                dest.ProductId,
                opt => opt.MapFrom(mf => mf.ProductId.ToHashId()));

}
