using CatalogApi.Common;
using FluentValidation;

namespace CatalogApi.Models.ModelValidators;

public class ProductModelValidator : AbstractValidator<ProductModel>
{
    public ProductModelValidator()
    {
        // Name validation
        RuleFor(product => product.Name)
            .NotEmpty().WithMessage(Constants.ProductNameRequired)
            .MaximumLength(Constants.ProductNameMaxLength).WithMessage(Constants.ProductNameTooLong);

        // Description validation
        RuleFor(product => product.Description)
            .NotEmpty().WithMessage(Constants.ProductDescriptionRequired)
            .MaximumLength(Constants.ProductDescriptionMaxLength).WithMessage(Constants.ProductDescriptionTooLong);

        // Price validation
        RuleFor(product => product.Price)
            .GreaterThan(0).WithMessage(Constants.ProductPriceMustBeGreaterThanZero);

        // Stock validation
        RuleFor(product => product.Stock)
            .GreaterThanOrEqualTo(0).WithMessage(Constants.ProductStockMustBeNonNegative);
    }
}
