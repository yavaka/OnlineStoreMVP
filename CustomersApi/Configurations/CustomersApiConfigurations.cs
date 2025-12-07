using CustomersApi.Data.Repositories;
using CustomersApi.Models;
using CustomersApi.Models.ModelValidations;
using FluentValidation;

namespace CustomersApi.Configurations;

public static class CustomersApiConfigurations
{
    public static IServiceCollection AddCustomersApiConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();

        // Validators
        services.AddScoped<IValidator<CustomerModel>, CustomerModelValidations>();

        // Data Repositories
        services.AddScoped<ICustomerRepository, CustomerRepository>();

        return services;
    }
}
