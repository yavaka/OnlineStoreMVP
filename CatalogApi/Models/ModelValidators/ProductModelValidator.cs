using CatalogApi.Common;
using FluentValidation;

namespace CatalogApi.Models.ModelValidators;

public class ProductModelValidator : AbstractValidator<ProductModel>
{
    public ProductModelValidator()
    {
        RuleFor(product => product.Name)
            .NotEmpty().WithMessage(Constants.ProductNameRequired)
            .MaximumLength(Constants.ProductNameMaxLength).WithMessage(Constants.ProductNameTooLong);

        RuleFor(product => product.Description)
            .NotEmpty().WithMessage(Constants.ProductDescriptionRequired)
            .MaximumLength(Constants.ProductDescriptionMaxLength).WithMessage(Constants.ProductDescriptionTooLong);

        RuleFor(product => product.Price)
            .GreaterThan(0).WithMessage(Constants.ProductPriceMustBeGreaterThanZero);

        RuleFor(product => product.Stock)
            .GreaterThanOrEqualTo(0).WithMessage(Constants.ProductStockMustBeNonNegative);
    }
}
