using Carter;
using Carter.ModelBinding;
using DomainEventsExample.Domain.Entities;
using DomainEventsExample.Persistence;
using FluentValidation;
using MediatR;

namespace DomainEventsExample.Features.Products.Commands;
public class CreateProduct : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/products", async (IMediator mediator, CreateProductCommand command) =>
        {
            return await mediator.Send(command);
        })
        .WithName(nameof(CreateProduct))
        .WithTags(nameof(Product))
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status201Created);
    }

    public class CreateProductCommand : IRequest<IResult>
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
    }

    public class CreateProductHandler : IRequestHandler<CreateProductCommand, IResult>
    {
        private readonly MyDbContext _context;
        private readonly IValidator<CreateProductCommand> _validator;

        public CreateProductHandler(MyDbContext context, IValidator<CreateProductCommand> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<IResult> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var result = _validator.Validate(request);
            if (!result.IsValid)
            {
                return Results.ValidationProblem(result.GetValidationProblems());
            }

            var newProduct = new Product(0, request.Name, request.Description, request.Price);

            _context.Products.Add(newProduct);

            await _context.SaveChangesAsync();

            return Results.Created($"api/products/{newProduct.ProductId}", null);
        }
    }

    public class CreateProductValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductValidator()
        {
            RuleFor(r => r.Name).NotEmpty();
            RuleFor(r => r.Description).NotEmpty();
            RuleFor(r => r.Price).NotEmpty();
        }
    }
}
