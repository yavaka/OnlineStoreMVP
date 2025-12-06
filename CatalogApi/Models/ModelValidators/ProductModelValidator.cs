using FluentValidation;

namespace CatalogApi.Models.ModelValidators;

public class ProductModelValidator : AbstractValidator<ProductModel>
{
    public ProductModelValidator()
    {
        RuleFor(product => product.Name)
            .NotEmpty()
            .WithMessage("Product name is required.");

        RuleFor(product => product.Description)
            .NotEmpty()
            .WithMessage("Product description is required.");

        RuleFor(product => product.Price)
            .GreaterThan(0)
            .WithMessage("Product price must be positive.");

        RuleFor(product => product.Stock)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Product stock must be zero or positive.");
    }
}
