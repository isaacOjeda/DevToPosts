using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using MinimalAPIFluentValidation.Common.Attributes;

namespace MinimalAPIFluentValidation.Features;


public class CreateProductCommand
{
    public double Price { get; set; }
    public string Description { get; set; } = default!;
    public int CategoryId { get; set; }
}



public static class CreateProductHandler
{
    public static Ok Handler([Validate] CreateProductCommand request, ILogger<CreateProductCommand> logger)
    {
        // TODO: Save Entity...

        logger.LogInformation("Saving {0}", request.Description);

        return TypedResults.Ok();
    }
}


public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(r => r.Description).NotEmpty();
        RuleFor(r => r.Price).GreaterThan(0);
        RuleFor(r => r.CategoryId).GreaterThan(0);
    }
}