using CustomersApi.Common;
using FluentValidation;

namespace CustomersApi.Models.ModelValidations;

public class CustomerModelValidations : AbstractValidator<CustomerModel>
{
    public CustomerModelValidations()
    {
        // Name validation
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage(Constants.CustomerNameRequired)
            .MaximumLength(Constants.CustomerNameMaxLength).WithMessage(Constants.CustomerNameTooLong);

        // Email validation
        RuleFor(c => c.Email)
            .NotEmpty().WithMessage(Constants.CustomerEmailRequired)
            .EmailAddress().WithMessage(Constants.CustomerEmailInvalid);

        // Address validation
        RuleFor(c => c.Address)
            .NotEmpty().WithMessage(Constants.CustomerAddressRequired)
            .MaximumLength(Constants.CustomerAddressMaxLength).WithMessage(Constants.CustomerAddressTooLong);
    }
}
