using CatalogApi.Models;
using CatalogApi.Models.ModelValidators;
using FluentValidation;

namespace CatalogApi.Configurations;

public static class CatalogApiConfigurations
{
    public static IServiceCollection AddCatalogApiConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();

        services.AddScoped<IValidator<ProductModel>, ProductModelValidator>();

        return services;
    }
}
