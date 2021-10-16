using FluentValidation;
using MinimalApis.Api.Entities;

namespace MinimalApis.Api.Features.Products
{
    public class ProductValidator : AbstractValidator<Product>
    {
        public ProductValidator()
        {
            RuleFor(q => q.Description).NotEmpty();
            RuleFor(q => q.Price).NotNull();
        }
    }
}
