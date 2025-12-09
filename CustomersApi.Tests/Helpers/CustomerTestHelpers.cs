using Bogus;
using CustomersApi.Models;

namespace CustomersApi.Tests.Helpers;

/// <summary>
/// Provides test data creation methods specific to CustomerModel using Bogus for realistic data generation.
/// </summary>
internal class CustomerTestHelpers
{
    // Create a Faker instance for CustomerModel with seed for reproducibility
    private static readonly Faker<CustomerModel> _customerFaker = new Faker<CustomerModel>()
        .RuleFor(c => c.Id, f => f.Random.Guid())
        .RuleFor(c => c.Name, f => f.Person.FullName)
        .RuleFor(c => c.Email, f => f.Person.Email)
        .RuleFor(c => c.Address, f => f.Address.FullAddress())
        .UseSeed(12345); // Seed for reproducible test data

    /// <summary>
    /// Creates a test CustomerModel with realistic fake data using Bogus.
    /// </summary>
    /// <param name="id">Optional customer ID. If null, a new Guid is generated.</param>
    /// <param name="name">Optional customer name. If null, a realistic full name is generated.</param>
    /// <param name="email">Optional customer email. If null, a realistic email is generated.</param>
    /// <param name="address">Optional customer address. If null, a realistic address is generated.</param>
    /// <returns>A CustomerModel instance with realistic test data.</returns>
    public static CustomerModel CreateTestCustomer(
        Guid? id = null,
        string? name = null,
        string? email = null,
        string? address = null)
    {
        var customer = _customerFaker.Generate();

        // Override with specific values if provided
        if (id.HasValue)
            customer.Id = id.Value;
        if (string.IsNullOrEmpty(name) is false)
            customer.Name = name;
        if (string.IsNullOrEmpty(email) is false)
            customer.Email = email;
        if (string.IsNullOrEmpty(address) is false)
            customer.Address = address;

        return customer;
    }

    /// <summary>
    /// Creates a list of test customers with varied realistic data.
    /// </summary>
    /// <param name="count">Number of customers to create.</param>
    /// <returns>A list of CustomerModel instances with realistic test data.</returns>
    public static List<CustomerModel> CreateTestCustomers(int count) => _customerFaker.Generate(count);

    /// <summary>
    /// Creates a customer with invalid data for validation testing.
    /// </summary>
    /// <returns>A CustomerModel with invalid data (empty name, invalid email, etc.).</returns>
    public static CustomerModel CreateInvalidCustomer() 
        => new()
        {
            Id = Guid.NewGuid(),
            Name = string.Empty, // Invalid: empty name
            Email = "invalid-email", // Invalid: not a valid email format
            Address = string.Empty // Invalid: empty address
        };
}
