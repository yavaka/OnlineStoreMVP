# Unit Testing Blueprint for Catalog and Customer APIs

## Overview

This plan establishes a comprehensive unit testing framework for `CatalogApi` and `CustomersApi` following .NET testing best practices. The tests will cover all controller endpoints, validation logic, exception handling, and edge cases.

## Architecture Considerations

### Current State Analysis

- Both controllers use **static in-memory lists** (`_products` and `_customers`) which create test isolation challenges
- Controllers depend on `ILogger<T>` and `IValidator<T>` (FluentValidation)
- Controllers inherit from `BaseController` which handles exceptions via `HandleException` method
- Validation uses FluentValidation with custom validators
- `ICustomerRepository` already exists in `CustomersApi/Data/Repositories/` with proper implementation pattern
- Test projects `CatalogApi.Tests` and `CustomersApi.Tests` already exist

### Testing Strategy

- **Unit Tests**: Test controllers in isolation with mocked dependencies
- **Test Isolation**: Each test should be independent and not rely on shared state
- **Mocking**: Use Moq for `ILogger`, `IValidator`, and repository interfaces
- **Assertions**: Use FluentAssertions for readable test assertions
- **Shared Utilities**: Create a common test utilities project to avoid code duplication

## Implementation Plan

### 1. Create Shared Test Utilities Project

**Problem**: Common test helper methods (like `CreateMockLogger`, `CreateMockValidator`) would be duplicated across multiple test projects.

**Solution**: Create a shared test utilities project that both test projects can reference.

**Create `OnlineStoreMVP.TestUtilities` project:**

- Location: `tests/OnlineStoreMVP.TestUtilities/`
- Project type: Class Library (`Microsoft.NET.Sdk`)
- Target framework: `net10.0` (matching API projects)
- Purpose: Contains shared test helpers, mock factories, and common test utilities

**Benefits:**
- ✅ DRY (Don't Repeat Yourself) principle
- ✅ Single source of truth for common test utilities
- ✅ Easier maintenance - update once, all tests benefit
- ✅ Consistent test helper implementations across all test projects
- ✅ Can be extended for future test projects (OrdersApi.Tests, PaymentsApi.Tests, etc.)

**Project Structure:**
```
tests/
├── OnlineStoreMVP.TestUtilities/
│   ├── OnlineStoreMVP.TestUtilities.csproj
│   ├── Helpers/
│   │   ├── MockHelpers.cs          # Common mock creation methods
│   │   └── ValidationHelpers.cs    # FluentValidation mock helpers
│   └── Extensions/
│       └── FluentAssertionsExtensions.cs  # Custom assertion extensions (optional)
├── CatalogApi.Tests/
│   ├── CatalogApi.Tests.csproj
│   ├── Controllers/
│   │   └── ProductsControllerTests.cs
│   └── Helpers/
│       └── ProductTestHelpers.cs   # Product-specific test data
└── CustomersApi.Tests/
    ├── CustomersApi.Tests.csproj
    ├── Controllers/
    │   └── CustomersControllerTests.cs
    └── Helpers/
        └── CustomerTestHelpers.cs  # Customer-specific test data
```

### 2. Install Required NuGet Packages

**For `OnlineStoreMVP.TestUtilities` project:**
- `Moq` - Mocking framework
- `FluentValidation` - For validation mock helpers
- `Microsoft.Extensions.Logging.Abstractions` - For ILogger mocks
- **`Bogus`** - **REQUIRED** - Fake data generator for creating realistic test data

**For both test projects (`CatalogApi.Tests` and `CustomersApi.Tests`):**
- `xunit` - Testing framework
- `xunit.runner.visualstudio` - Test runner for Visual Studio
- `Moq` - Mocking framework
- **`FluentAssertions`** - **REQUIRED** - Fluent assertion library for readable, expressive assertions
- **`Bogus`** - **REQUIRED** - Fake data generator for creating realistic test data
- `Microsoft.AspNetCore.Mvc.Testing` - For integration testing (optional, for future)
- `Microsoft.NET.Test.Sdk` - Test SDK
- **Project Reference**: `OnlineStoreMVP.TestUtilities` (to use shared helpers)

**Important Packages:**

**FluentAssertions** is **mandatory** for this testing framework. It provides:
- More readable and expressive assertions
- Better error messages when tests fail
- Fluent API that reads like natural language
- Comprehensive assertion methods for all .NET types

**Bogus** is **mandatory** for generating realistic test data. It provides:
- Realistic fake data generation (names, emails, addresses, etc.)
- Consistent data generation with seed support
- Fluent API for building custom data generators
- Reduces hardcoded test data and makes tests more maintainable
- Better test coverage with varied, realistic data scenarios

### 3. Shared Test Utilities Implementation

**`OnlineStoreMVP.TestUtilities/Helpers/MockHelpers.cs`:**

```csharp
using Microsoft.Extensions.Logging;
using Moq;

namespace OnlineStoreMVP.TestUtilities.Helpers;

/// <summary>
/// Provides common mock creation methods for testing.
/// </summary>
public static class MockHelpers
{
    /// <summary>
    /// Creates a mocked ILogger instance for the specified type.
    /// </summary>
    /// <typeparam name="T">The type for which to create the logger.</typeparam>
    /// <returns>A mocked ILogger instance.</returns>
    public static Mock<ILogger<T>> CreateMockLogger<T>()
    {
        return new Mock<ILogger<T>>();
    }

    /// <summary>
    /// Creates a mocked ILogger instance with verification setup.
    /// </summary>
    /// <typeparam name="T">The type for which to create the logger.</typeparam>
    /// <param name="logLevel">The expected log level.</param>
    /// <param name="times">The number of times the log should be called.</param>
    /// <returns>A mocked ILogger instance configured for verification.</returns>
    public static Mock<ILogger<T>> CreateMockLoggerWithVerification<T>(
        LogLevel logLevel = LogLevel.Error,
        Times? times = null)
    {
        var mockLogger = new Mock<ILogger<T>>();
        mockLogger.Setup(x => x.Log(
            It.Is<LogLevel>(l => l == logLevel),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)))
            .Verifiable();
        
        return mockLogger;
    }
}
```

**`OnlineStoreMVP.TestUtilities/Helpers/ValidationHelpers.cs`:**

```csharp
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace OnlineStoreMVP.TestUtilities.Helpers;

/// <summary>
/// Provides helpers for creating FluentValidation mocks in tests.
/// </summary>
public static class ValidationHelpers
{
    /// <summary>
    /// Creates a mocked IValidator that always returns valid.
    /// </summary>
    /// <typeparam name="T">The type to validate.</typeparam>
    /// <returns>A mocked IValidator that returns valid results.</returns>
    public static Mock<IValidator<T>> CreateValidValidator<T>()
    {
        var mockValidator = new Mock<IValidator<T>>();
        mockValidator.Setup(v => v.Validate(It.IsAny<T>()))
            .Returns(new ValidationResult());
        return mockValidator;
    }

    /// <summary>
    /// Creates a mocked IValidator with custom validation errors.
    /// </summary>
    /// <typeparam name="T">The type to validate.</typeparam>
    /// <param name="errors">Dictionary of property names and their error messages.</param>
    /// <returns>A mocked IValidator that returns the specified validation errors.</returns>
    public static Mock<IValidator<T>> CreateInvalidValidator<T>(
        Dictionary<string, string[]> errors)
    {
        var mockValidator = new Mock<IValidator<T>>();
        var failures = errors.SelectMany(e => 
            e.Value.Select(v => new ValidationFailure(e.Key, v))).ToList();
        
        mockValidator.Setup(v => v.Validate(It.IsAny<T>()))
            .Returns(new ValidationResult(failures));
        
        return mockValidator;
    }

    /// <summary>
    /// Creates a mocked IValidator with a single validation error.
    /// </summary>
    /// <typeparam name="T">The type to validate.</typeparam>
    /// <param name="propertyName">The property name with the error.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <returns>A mocked IValidator that returns the specified validation error.</returns>
    public static Mock<IValidator<T>> CreateInvalidValidator<T>(
        string propertyName, 
        string errorMessage)
    {
        return CreateInvalidValidator<T>(new Dictionary<string, string[]>
        {
            { propertyName, new[] { errorMessage } }
        });
    }
}
```

### 4. API-Specific Test Helpers with Bogus

**Bogus Integration**: Use Bogus to generate realistic, varied test data instead of hardcoded values. This makes tests more robust and maintainable.

**`CatalogApi.Tests/Helpers/ProductTestHelpers.cs`:**

```csharp
using Bogus;
using CatalogApi.Models;

namespace CatalogApi.Tests.Helpers;

/// <summary>
/// Provides test data creation methods specific to ProductModel using Bogus for realistic data generation.
/// </summary>
public static class ProductTestHelpers
{
    // Create a Faker instance for ProductModel with seed for reproducibility
    private static readonly Faker<ProductModel> ProductFaker = new Faker<ProductModel>()
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
        var product = ProductFaker.Generate();
        
        // Override with specific values if provided
        if (id.HasValue)
            product.Id = id.Value;
        if (!string.IsNullOrEmpty(name))
            product.Name = name;
        if (!string.IsNullOrEmpty(description))
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
    public static List<ProductModel> CreateTestProducts(int count)
    {
        return ProductFaker.Generate(count);
    }

    /// <summary>
    /// Creates a product with invalid data for validation testing.
    /// </summary>
    /// <returns>A ProductModel with invalid data (negative price, empty name, etc.).</returns>
    public static ProductModel CreateInvalidProduct()
    {
        return new ProductModel
        {
            Id = Guid.NewGuid(),
            Name = string.Empty, // Invalid: empty name
            Description = string.Empty, // Invalid: empty description
            Price = -10.00m, // Invalid: negative price
            Stock = -5 // Invalid: negative stock
        };
    }
}
```

**`CustomersApi.Tests/Helpers/CustomerTestHelpers.cs`:**

```csharp
using Bogus;
using CustomersApi.Models;

namespace CustomersApi.Tests.Helpers;

/// <summary>
/// Provides test data creation methods specific to CustomerModel using Bogus for realistic data generation.
/// </summary>
public static class CustomerTestHelpers
{
    // Create a Faker instance for CustomerModel with seed for reproducibility
    private static readonly Faker<CustomerModel> CustomerFaker = new Faker<CustomerModel>()
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
        var customer = CustomerFaker.Generate();
        
        // Override with specific values if provided
        if (id.HasValue)
            customer.Id = id.Value;
        if (!string.IsNullOrEmpty(name))
            customer.Name = name;
        if (!string.IsNullOrEmpty(email))
            customer.Email = email;
        if (!string.IsNullOrEmpty(address))
            customer.Address = address;
            
        return customer;
    }

    /// <summary>
    /// Creates a list of test customers with varied realistic data.
    /// </summary>
    /// <param name="count">Number of customers to create.</param>
    /// <returns>A list of CustomerModel instances with realistic test data.</returns>
    public static List<CustomerModel> CreateTestCustomers(int count)
    {
        return CustomerFaker.Generate(count);
    }

    /// <summary>
    /// Creates a customer with invalid data for validation testing.
    /// </summary>
    /// <returns>A CustomerModel with invalid data (empty name, invalid email, etc.).</returns>
    public static CustomerModel CreateInvalidCustomer()
    {
        return new CustomerModel
        {
            Id = Guid.NewGuid(),
            Name = string.Empty, // Invalid: empty name
            Email = "invalid-email", // Invalid: not a valid email format
            Address = string.Empty // Invalid: empty address
        };
    }

    /// <summary>
    /// Creates a customer with name exceeding maximum length for validation testing.
    /// </summary>
    /// <returns>A CustomerModel with name exceeding 100 characters.</returns>
    public static CustomerModel CreateCustomerWithLongName()
    {
        var faker = new Faker();
        return new CustomerModel
        {
            Id = Guid.NewGuid(),
            Name = faker.Random.String(101), // Invalid: exceeds 100 character limit
            Email = faker.Person.Email,
            Address = faker.Address.FullAddress()
        };
    }

    /// <summary>
    /// Creates a customer with address exceeding maximum length for validation testing.
    /// </summary>
    /// <returns>A CustomerModel with address exceeding 200 characters.</returns>
    public static CustomerModel CreateCustomerWithLongAddress()
    {
        var faker = new Faker();
        return new CustomerModel
        {
            Id = Guid.NewGuid(),
            Name = faker.Person.FullName,
            Email = faker.Person.Email,
            Address = faker.Random.String(201) // Invalid: exceeds 200 character limit
        };
    }
}
```

**Bogus Benefits:**

- ✅ **Realistic Data**: Generates realistic names, emails, addresses, product names, etc.
- ✅ **Variety**: Each test run can have different data, catching edge cases
- ✅ **Reproducibility**: Using `.UseSeed()` ensures consistent data when needed
- ✅ **Less Maintenance**: No hardcoded test data to update
- ✅ **Better Coverage**: Varied data helps discover bugs that fixed test data might miss
- ✅ **Fluent API**: Easy to customize data generation rules

### 5. Refactor Controllers for Testability

**Critical Issue**: The static lists (`_products` and `_customers`) prevent proper test isolation.

**Solution**: Extract data access to repository interfaces that can be mocked.

**Repository Pattern Implementation:**

- Create `IProductRepository` interface in `CatalogApi/Data/Repositories/`
- Use existing `ICustomerRepository` interface in `CustomersApi/Data/Repositories/`
- Inject these interfaces into controllers via constructor injection
- Implement concrete repository classes for production
- Mock interfaces in tests

**Repository Best Practices (Already Implemented in CustomerRepository):**

- ✅ **Finding by ID should be handled in the repository**, not the controller
- ✅ `UpdateAsync(Guid id, TModel model)` should internally find the entity by ID using `GetByIdAsync`
- ✅ Repository methods return nullable types (`TModel?`) when entity might not exist
- ✅ Controllers check for `null` results and throw `NotFoundException` when appropriate
- ✅ This approach ensures single database call, better encapsulation, and atomic operations
- ✅ `DeleteAsync` follows the same pattern - finds internally and returns `bool` for success/failure

**Example Repository Pattern (as implemented in CustomerRepository):**

```csharp
public async Task<CustomerModel?> UpdateAsync(Guid id, CustomerModel customer)
{
    var existingCustomer = await GetByIdAsync(id);
    if (existingCustomer == null)
    {
        return null; // Not found - repository handles finding
    }
    
    // Update properties
    existingCustomer.Name = customer.Name;
    existingCustomer.Email = customer.Email;
    existingCustomer.Address = customer.Address;
    
    return existingCustomer;
}
```

### 6. Exception Handling Strategy

**Three-Layer Exception Handling Pattern:**

**Repository Layer (Data Access):**
- ✅ Catch database-specific exceptions (e.g., `SqlException`) and transform to domain exceptions
- ✅ Let other exceptions bubble up to service/controller layer
- ✅ Return `null` or `false` for "not found" scenarios (don't throw exceptions for expected cases)

**Service Layer (Business Logic - if added in future):**
- ✅ Use try-catch **only** to:
  - Transform exceptions (e.g., `SqlException` → `ValidationException`)
  - Add contextual logging before re-throwing
  - Handle recoverable errors gracefully
- ❌ **Do NOT** catch exceptions just to re-throw without adding value
- ❌ **Do NOT** catch exceptions the service cannot handle (let controller handle HTTP responses)

**Controller Layer:**
- ✅ Catch **all exceptions** and convert to HTTP responses via `BaseController.HandleException`
- ✅ This is the appropriate place for comprehensive exception handling
- ✅ Controllers are responsible for HTTP status codes and error response formatting

**Example Exception Handling:**

```csharp
// Repository - minimal exception handling
public async Task<CustomerModel?> UpdateAsync(Guid id, CustomerModel customer)
{
    try
    {
        var existing = await GetByIdAsync(id);
        if (existing == null) return null;
        // Update logic
    }
    catch (SqlException ex) when (ex.Number == 2627) // Unique constraint
    {
        throw new ValidationException(new Dictionary<string, string[]>
        {
            { "Email", new[] { "Email already exists." } }
        });
    }
    // Let other exceptions bubble up
}

// Controller - comprehensive exception handling
[HttpPut("{id}")]
public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] CustomerModel customer)
{
    try
    {
        // Validation
        var validationResult = _validator.Validate(customer);
        if (!validationResult.IsValid) { /* throw ValidationException */ }
        
        // Repository call
        var updated = await _repository.UpdateAsync(id, customer);
        if (updated == null)
        {
            throw new NotFoundException(nameof(CustomerModel), id);
        }
        
        return NoContent();
    }
    catch (Exception ex)
    {
        return HandleException(ex, _logger, nameof(UpdateCustomer));
    }
}
```

### 7. FluentAssertions Usage Guide

**FluentAssertions is the primary assertion library** for all tests. It provides a fluent, readable API for assertions.

**Key Benefits:**
- ✅ More readable test code: `result.Should().BeOfType<OkObjectResult>()` vs `Assert.IsType<OkObjectResult>(result)`
- ✅ Better error messages when assertions fail
- ✅ Natural language-like syntax
- ✅ Comprehensive assertion methods for collections, objects, exceptions, etc.

**Common FluentAssertions Patterns:**

```csharp
// Type assertions
result.Should().BeOfType<OkObjectResult>();
result.Should().BeOfType<NotFoundObjectResult>();

// Status code assertions
var okResult = result as OkObjectResult;
okResult.StatusCode.Should().Be(200);
okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

// Value assertions
okResult.Value.Should().BeEquivalentTo(expectedProduct);
okResult.Value.Should().NotBeNull();
okResult.Value.Should().Be(expectedValue);

// Collection assertions
var products = okResult.Value as List<ProductModel>;
products.Should().NotBeNull();
products.Should().HaveCount(2);
products.Should().Contain(p => p.Id == productId);
products.Should().BeEquivalentTo(expectedProducts);

// Exception assertions
action.Should().Throw<NotFoundException>();
action.Should().Throw<NotFoundException>()
    .WithMessage("Entity \"ProductModel\" (*) was not found.");

// Object property assertions
product.Should().NotBeNull();
product.Name.Should().Be("Test Product");
product.Price.Should().BeGreaterThan(0);
product.Stock.Should().BeGreaterThanOrEqualTo(0);

// HTTP result assertions
result.Should().BeOfType<CreatedAtActionResult>();
var createdResult = result as CreatedAtActionResult;
createdResult.StatusCode.Should().Be(201);
createdResult.RouteValues["id"].Should().Be(productId);
```

**Always use FluentAssertions instead of xUnit's Assert class** for consistency and better readability.

**Comparison: xUnit Assert vs FluentAssertions**

```csharp
// ❌ xUnit Assert (NOT RECOMMENDED)
Assert.IsType<OkObjectResult>(result);
var okResult = Assert.IsType<OkObjectResult>(result);
Assert.Equal(200, okResult.StatusCode);
Assert.NotNull(okResult.Value);
Assert.Equal(expectedProduct.Id, ((ProductModel)okResult.Value).Id);

// ✅ FluentAssertions (RECOMMENDED - Use this)
result.Should().BeOfType<OkObjectResult>();
var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
okResult.StatusCode.Should().Be(200);
okResult.Value.Should().NotBeNull();
okResult.Value.Should().BeEquivalentTo(expectedProduct); // Deep comparison
```

**Why FluentAssertions is Better:**
- More readable: reads like natural language
- Better error messages: shows exactly what was expected vs actual
- Less verbose: fewer lines of code
- More powerful: `BeEquivalentTo()` does deep object comparison automatically
- Consistent API: same pattern for all assertion types

### 8. Test Class Structure

For each controller, create test classes organized by operation:

**CatalogApi.Tests:**

- `ProductsControllerTests.cs` - Main test class
  - `GetProductsTests` - Nested class for GET all products
  - `GetProductTests` - Nested class for GET by ID
  - `CreateProductTests` - Nested class for POST
  - `UpdateProductTests` - Nested class for PUT
  - `DeleteProductTests` - Nested class for DELETE

**CustomersApi.Tests:**

- `CustomersControllerTests.cs` - Main test class
  - `GetCustomersTests` - Nested class for GET all customers
  - `GetCustomerTests` - Nested class for GET by ID
  - `CreateCustomerTests` - Nested class for POST
  - `UpdateCustomerTests` - Nested class for PUT
  - `DeleteCustomerTests` - Nested class for DELETE

### 9. Test Coverage Requirements

#### ProductsController Test Scenarios

**GetProducts:**

- ✅ Returns 200 OK with list of products
- ✅ Returns empty list when no products exist
- ✅ Handles exceptions correctly

**GetProduct:**

- ✅ Returns 200 OK with product when found
- ✅ Returns 404 NotFound when product doesn't exist (repository returns null)
- ✅ Handles exceptions correctly

**CreateProduct:**

- ✅ Returns 201 Created with valid product
- ✅ Generates new Guid for product ID
- ✅ Returns 400 BadRequest with validation errors
- ✅ Validates all required fields (Name, Description, Price, Stock)
- ✅ Validates Price > 0
- ✅ Validates Stock >= 0
- ✅ Handles exceptions correctly

**UpdateProduct:**

- ✅ Returns 204 NoContent when update succeeds
- ✅ Returns 404 NotFound when product doesn't exist (repository returns null)
- ✅ Returns 400 BadRequest with validation errors
- ✅ Verifies repository.UpdateAsync is called with correct parameters
- ✅ Handles exceptions correctly

**DeleteProduct:**

- ✅ Returns 204 NoContent when deletion succeeds
- ✅ Returns 404 NotFound when product doesn't exist (repository returns false)
- ✅ Handles exceptions correctly

#### CustomersController Test Scenarios

**GetCustomers:**

- ✅ Returns 200 OK with list of customers
- ✅ Returns empty list when no customers exist
- ✅ Handles exceptions correctly

**GetCustomer:**

- ✅ Returns 200 OK with customer when found
- ✅ Returns 404 NotFound when customer doesn't exist (repository returns null)
- ✅ Handles exceptions correctly

**CreateCustomer:**

- ✅ Returns 201 Created with valid customer
- ✅ Generates new Guid for customer ID
- ✅ Returns 400 BadRequest with validation errors
- ✅ Validates Name (required, max 100 chars)
- ✅ Validates Email (required, valid format)
- ✅ Validates Address (required, max 200 chars)
- ✅ Handles exceptions correctly

**UpdateCustomer:**

- ✅ Returns 204 NoContent when update succeeds
- ✅ Returns 404 NotFound when customer doesn't exist (repository returns null)
- ✅ Returns 400 BadRequest with validation errors
- ✅ Verifies repository.UpdateAsync is called with correct parameters
- ✅ Verifies repository finds customer internally (mocks GetByIdAsync within UpdateAsync)
- ✅ Handles exceptions correctly

**DeleteCustomer:**

- ✅ Returns 204 NoContent when deletion succeeds
- ✅ Returns 404 NotFound when customer doesn't exist (repository returns false)
- ✅ Handles exceptions correctly

### 10. Example Test Structure Using Shared Utilities

**Complete Example for ProductsControllerTests:**

```csharp
using CatalogApi.Controllers;
using CatalogApi.Data.Repositories;
using CatalogApi.Models;
using CatalogApi.Tests.Helpers;
using FluentAssertions; // REQUIRED - Primary assertion library
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OnlineStoreMVP.ServiceDefaults.Exceptions;
using OnlineStoreMVP.TestUtilities.Helpers; // Shared utilities
using Xunit;

namespace CatalogApi.Tests.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<ILogger<ProductsController>> _mockLogger;
    private readonly Mock<IValidator<ProductModel>> _mockValidator;
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        // Use shared utilities for common mocks
        _mockLogger = MockHelpers.CreateMockLogger<ProductsController>();
        _mockValidator = ValidationHelpers.CreateValidValidator<ProductModel>();
        _mockRepository = new Mock<IProductRepository>();
        _controller = new ProductsController(
            _mockLogger.Object, 
            _mockValidator.Object,
            _mockRepository.Object);
    }

    public class GetProductsTests : ProductsControllerTests
    {
        [Fact]
        public async Task GetProducts_WhenProductsExist_ReturnsOkWithProducts()
        {
            // Arrange - Using Bogus to generate realistic test data
            var products = ProductTestHelpers.CreateTestProducts(2);
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(products);

            // Act
            var result = await _controller.GetProducts();

            // Assert - Using FluentAssertions
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(products);
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetProducts_WhenNoProductsExist_ReturnsOkWithEmptyList()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<ProductModel>());

            // Act
            var result = await _controller.GetProducts();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(new List<ProductModel>());
        }
    }

    public class GetProductTests : ProductsControllerTests
    {
        [Fact]
        public async Task GetProduct_WithValidId_ReturnsOkWithProduct()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var expectedProduct = ProductTestHelpers.CreateTestProduct(productId);
            _mockRepository.Setup(r => r.GetByIdAsync(productId))
                .ReturnsAsync(expectedProduct);

            // Act
            var result = await _controller.GetProduct(productId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(expectedProduct);
            _mockRepository.Verify(r => r.GetByIdAsync(productId), Times.Once);
        }

        [Fact]
        public async Task GetProduct_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var productId = Guid.NewGuid();
            _mockRepository.Setup(r => r.GetByIdAsync(productId))
                .ReturnsAsync((ProductModel?)null);

            // Act
            var result = await _controller.GetProduct(productId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Value.Should().BeOfType<ErrorResponse>();
        }
    }

    public class UpdateProductTests : ProductsControllerTests
    {
        [Fact]
        public async Task UpdateProduct_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var updatedProduct = ProductTestHelpers.CreateTestProduct();
            var existingProduct = ProductTestHelpers.CreateTestProduct(productId);
            
            _mockValidator.Setup(v => v.Validate(updatedProduct))
                .Returns(new FluentValidation.Results.ValidationResult());
            _mockRepository.Setup(r => r.UpdateAsync(productId, updatedProduct))
                .ReturnsAsync(existingProduct);

            // Act
            var result = await _controller.UpdateProduct(productId, updatedProduct);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _mockRepository.Verify(r => r.UpdateAsync(productId, updatedProduct), Times.Once);
        }

        [Fact]
        public async Task UpdateProduct_WhenProductNotFound_ReturnsNotFound()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var updatedProduct = ProductTestHelpers.CreateTestProduct();
            
            _mockValidator.Setup(v => v.Validate(updatedProduct))
                .Returns(new FluentValidation.Results.ValidationResult());
            _mockRepository.Setup(r => r.UpdateAsync(productId, updatedProduct))
                .ReturnsAsync((ProductModel?)null);

            // Act
            var result = await _controller.UpdateProduct(productId, updatedProduct);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task UpdateProduct_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var invalidProduct = ProductTestHelpers.CreateTestProduct();
            invalidProduct.Price = -10; // Invalid price
            
            // Use shared validation helper for invalid validator
            var mockInvalidValidator = ValidationHelpers.CreateInvalidValidator<ProductModel>(
                "Price", 
                "Product price must be positive.");
            
            var controllerWithInvalidValidator = new ProductsController(
                _mockLogger.Object,
                mockInvalidValidator.Object,
                _mockRepository.Object);

            // Act
            var result = await controllerWithInvalidValidator.UpdateProduct(productId, invalidProduct);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Guid>(), It.IsAny<ProductModel>()), Times.Never);
        }
    }
}
```

### 11. Files to Create

**OnlineStoreMVP.TestUtilities (NEW - Shared Project):**

- `OnlineStoreMVP.TestUtilities.csproj`
- `Helpers/MockHelpers.cs` - Common mock creation methods
- `Helpers/ValidationHelpers.cs` - FluentValidation mock helpers
- **Note**: Bogus is referenced by individual test projects, not the shared utilities project

**CatalogApi.Tests:**

- `CatalogApi.Tests.csproj` (add reference to `OnlineStoreMVP.TestUtilities`)
- `ProductsControllerTests.cs`
- `Helpers/ProductTestHelpers.cs` - Product-specific test data creation

**CustomersApi.Tests:**

- `CustomersApi.Tests.csproj` (add reference to `OnlineStoreMVP.TestUtilities`)
- `CustomersControllerTests.cs`
- `Helpers/CustomerTestHelpers.cs` - Customer-specific test data creation

**CatalogApi (Refactoring):**

- `Data/Repositories/IProductRepository.cs` (interface)
- `Data/Repositories/ProductRepository.cs` (implementation)

**CustomersApi (Already Exists):**

- ✅ `Data/Repositories/ICustomerRepository.cs` (interface)
- ✅ `Data/Repositories/CustomerRepository.cs` (implementation)

### 12. Solution File Updates

- Add `OnlineStoreMVP.TestUtilities` project to `OnlineStoreMVP.sln` in the `tests` solution folder
- Ensure `CatalogApi.Tests` and `CustomersApi.Tests` have project references to `OnlineStoreMVP.TestUtilities`
- Configure build dependencies if needed

### 13. Code Organization Best Practices

- Follow **Arrange-Act-Assert (AAA)** pattern
- Use descriptive test method names: `MethodName_Scenario_ExpectedResult`
- Group related tests using nested classes
- Use `[Fact]` for single test cases and `[Theory]` for parameterized tests
- Add XML documentation comments to test methods explaining the scenario
- Keep tests focused on one behavior per test
- Use meaningful variable names in tests
- **Use shared utilities** for common operations (mocking, validation)
- **Keep API-specific helpers** in their respective test projects

### 14. Testing Best Practices to Follow

1. **Isolation**: Each test should be independent and not affect others
2. **Fast Execution**: Unit tests should run quickly (< 1 second each)
3. **Deterministic**: Tests should produce consistent results
4. **Clear Naming**: Test names should clearly describe what is being tested
5. **Single Responsibility**: Each test should verify one behavior
6. **Mock External Dependencies**: Mock ILogger, IValidator, and repository interfaces
7. **Test Edge Cases**: Include boundary conditions and null checks
8. **Verify HTTP Status Codes**: Assert correct status codes in responses
9. **Verify Response Content**: Check response body structure and data
10. **Exception Scenarios**: Test both happy paths and error paths
11. **Verify Mock Interactions**: Use `Verify()` to ensure methods are called correctly
12. **Avoid Testing Framework Code**: Don't test ASP.NET Core or FluentValidation internals
13. **Always Use FluentAssertions**: **MANDATORY** - Use FluentAssertions for all assertions instead of xUnit's Assert class. This provides better readability and error messages
14. **Test Repository Integration**: Mock repository methods to return expected data
15. **FluentAssertions Patterns**: Use `.Should().BeOfType<>()`, `.Should().BeEquivalentTo()`, `.Should().NotBeNull()` for consistent, readable assertions
16. **DRY Principle**: Use shared test utilities for common operations, keep API-specific helpers separate
17. **Use Bogus for Test Data**: **MANDATORY** - Always use Bogus to generate realistic test data instead of hardcoded values. This provides better test coverage and maintainability
18. **Bogus Seed for Reproducibility**: Use `.UseSeed()` when you need reproducible test data for specific test scenarios

## Success Criteria

- All CRUD operations have comprehensive test coverage
- All validation scenarios are tested
- All exception handling paths are tested
- Tests run in isolation without shared state
- Test execution time is fast (< 5 seconds for full suite)
- Code coverage is > 80% for controllers
- All tests pass consistently
- Tests follow AAA pattern and are well-documented
- Repository methods are properly mocked and verified
- Exception handling is tested at controller level
- HTTP status codes are verified for all scenarios
- **Shared test utilities are used consistently across all test projects**
- **No code duplication in test helper methods**
- **Bogus is used for all test data generation** - no hardcoded test values
- **Test data is realistic and varied** - better test coverage

## Additional Notes

### Repository Mocking Strategy

When testing `UpdateAsync` and `DeleteAsync`, remember that these methods internally call `GetByIdAsync`. When mocking:

- For `UpdateAsync`: Mock `UpdateAsync` directly - it will handle finding internally
- For `DeleteAsync`: Mock `DeleteAsync` directly - it will handle finding internally
- The repository implementation handles the "find then update/delete" pattern internally

### Controller Refactoring Checklist

Before writing tests, ensure controllers are refactored to:

- ✅ Inject repository interface via constructor
- ✅ Remove static lists
- ✅ Check for null results from repository and throw `NotFoundException`
- ✅ Maintain existing validation logic
- ✅ Keep exception handling via `BaseController.HandleException`

### Shared Utilities Benefits

**Why use a shared test utilities project:**

1. **DRY Principle**: Write once, use everywhere
2. **Consistency**: All test projects use the same helper implementations
3. **Maintainability**: Update helper logic in one place
4. **Extensibility**: Easy to add new test projects (OrdersApi.Tests, PaymentsApi.Tests) that can reuse utilities
5. **Separation of Concerns**: 
   - Shared utilities = Common operations (mocking, validation)
   - API-specific helpers = Domain-specific test data (ProductModel, CustomerModel)

### Bogus Usage Examples

**Basic Usage:**
```csharp
// Generate a single product with realistic data
var product = ProductTestHelpers.CreateTestProduct();

// Generate multiple products with varied data
var products = ProductTestHelpers.CreateTestProducts(10);

// Override specific properties when needed
var specificProduct = ProductTestHelpers.CreateTestProduct(
    name: "Custom Product Name",
    price: 199.99m
);
```

**Validation Testing:**
```csharp
// Test with invalid data
var invalidProduct = ProductTestHelpers.CreateInvalidProduct();

// Test with boundary conditions
var customerWithLongName = CustomerTestHelpers.CreateCustomerWithLongName();
var customerWithLongAddress = CustomerTestHelpers.CreateCustomerWithLongAddress();
```

**Benefits of Using Bogus:**
- Tests are more realistic and closer to production data
- Less brittle - no hardcoded strings that might change
- Better edge case discovery with varied data
- Easier to maintain - update Faker rules in one place
- Consistent data generation across all tests

### Future Enhancements

- Integration tests using `Microsoft.AspNetCore.Mvc.Testing`
- Test coverage reporting with coverlet
- Performance tests for high-traffic endpoints
- Contract tests for API responses
- Additional shared utilities as needed (e.g., HTTP client mocks, database test fixtures)
- Custom Bogus rules for domain-specific data generation

