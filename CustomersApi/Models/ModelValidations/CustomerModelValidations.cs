using CustomersApi.Common;
using FluentValidation;

namespace CustomersApi.Models.ModelValidations;

public class CustomerModelValidations : AbstractValidator<CustomerModel>
{
    public CustomerModelValidations()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage(Constants.CustomerNameRequired)
            .MaximumLength(Constants.CustomerNameMaxLength).WithMessage(Constants.CustomerNameTooLong);

        RuleFor(c => c.Email)
            .NotEmpty().WithMessage(Constants.CustomerEmailRequired)
            .EmailAddress().WithMessage(Constants.CustomerEmailInvalid);

        RuleFor(c => c.Address)
            .NotEmpty().WithMessage(Constants.CustomerAddressRequired)
            .MaximumLength(Constants.CustomerAddressMaxLength).WithMessage(Constants.CustomerAddressTooLong);
    }
}
