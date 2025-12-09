using Bogus;
using CatalogApi.Models;

namespace CatalogApi.Tests.Helpers;

/// <summary>
/// Provides test data creation methods specific to ProductModel using Bogus for realistic data generation.
/// </summary>
public static class ProductTestHelpers
{
    /// <summary>
    /// Create a Faker instance for ProductModel with seed for reproducibility
    /// </summary>
    private static readonly Faker<ProductModel> _productFaker = new Faker<ProductModel>()
        .RuleFor(p => p.Id, f => f.Random.Guid())
        .RuleFor(p => p.Name, f => f.Commerce.ProductName())
        .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
        .RuleFor(p => p.Price, f => f.Random.Decimal(1.00m, 1000.00m))
        .RuleFor(p => p.Stock, f => f.Random.Int(0, 1000))
        .UseSeed(12345); // Seed for reproducible test data

    /// <summary>
    /// Creates a test ProductModel with realistic fake data using Bogus.
    /// </summary>
    /// <param name="id">Optional product ID. If null, a new Guid is generated.</param>
    /// <param name="name">Optional product name. If null, a realistic product name is generated.</param>
    /// <param name="description">Optional product description. If null, a realistic description is generated.</param>
    /// <param name="price">Optional product price. If null, a random price between 1.00 and 1000.00 is generated.</param>
    /// <param name="stock">Optional product stock. If null, a random stock between 0 and 1000 is generated.</param>
    /// <returns>A ProductModel instance with realistic test data.</returns>
    public static ProductModel CreateTestProduct(
        Guid? id = null,
        string? name = null,
        string? description = null,
        decimal? price = null,
        int? stock = null)
    {
        var product = _productFaker.Generate();

        // Override with specific values if provided
        if (id.HasValue)
            product.Id = id.Value;
        if (string.IsNullOrEmpty(name) is false)
            product.Name = name;
        if (string.IsNullOrEmpty(description) is false)
            product.Description = description;
        if (price.HasValue)
            product.Price = price.Value;
        if (stock.HasValue)
            product.Stock = stock.Value;

        return product;
    }

    /// <summary>
    /// Creates a list of test products with varied realistic data.
    /// </summary>
    /// <param name="count">Number of products to create.</param>
    /// <returns>A list of ProductModel instances with realistic test data.</returns>
    public static List<ProductModel> CreateTestProducts(int count) => _productFaker.Generate(count);

    /// <summary>
    /// Creates a product with invalid data for validation testing.
    /// </summary>
    /// <returns>A ProductModel with invalid data (negative price, empty name, etc.).</returns>
    public static ProductModel CreateInvalidProduct() 
        => new()
        {
            Id = Guid.NewGuid(),
            Name = string.Empty, // Invalid: empty name
            Description = string.Empty, // Invalid: empty description
            Price = -10.00m, // Invalid: negative price
            Stock = -5 // Invalid: negative stock
        };
}
