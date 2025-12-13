using CustomersApi.Data.Repositories;
using CustomersApi.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OnlineStoreMVP.ServiceDefaults.Common.Exceptions;
using OnlineStoreMVP.ServiceDefaults.Controllers;
using ValidationException = OnlineStoreMVP.ServiceDefaults.Common.Exceptions.ValidationException;

namespace CustomersApi.Controllers;

/// <summary>
/// API controller that manages customer records, providing endpoints to create, retrieve, update, and delete customers.
/// </summary>
/// <remarks>All endpoints in this controller require valid customer data and handle common error scenarios, such
/// as validation failures or missing records, by returning appropriate HTTP responses. The controller is intended to be
/// used as part of an ASP.NET Core Web API and follows RESTful conventions for resource management.</remarks>
/// <param name="logger">The logger used to record diagnostic and operational information for the controller.</param>
/// <param name="customerRepository">The repository used to access and manage customer data.</param>
/// <param name="validator">The validator used to ensure that customer models meet all required validation rules.</param>
[Route("api/[controller]")]
public class CustomersController(
    ILogger<CustomersController> logger,
    ICustomerRepository customerRepository,
    IValidator<CustomerModel> validator) : BaseController
{
    private readonly ILogger<CustomersController> _logger = logger;
    private readonly ICustomerRepository _customerRepository = customerRepository;
    private readonly IValidator<CustomerModel> _validator = validator;

    /// <summary>
    /// Creates a new customer record using the provided customer information.
    /// </summary>
    /// <param name="customer">The customer data to create. Must not be null and must satisfy all validation requirements.</param>
    /// <returns>An IActionResult that represents the result of the create operation. Returns a 201 Created response with the
    /// created customer if successful, or an error response if validation fails or an exception occurs.
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CustomerModel customer)
    {
        try
        {
            var validationResult = _validator.Validate(customer);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
                throw new ValidationException(errors);
            }

            var createdCustomer = await _customerRepository.AddAsync(customer);

            return CreatedAtAction(nameof(CreateCustomer), new { id = createdCustomer.Id }, createdCustomer);
        }
        catch (Exception ex)
        {
            return HandleException(ex, _logger, nameof(CreateCustomer));
        }
    }

    /// <summary>
    /// Updates an existing customer.
    /// </summary>
    /// <param name="id">The unique identifier of the customer to update.</param>
    /// <param name="updatedCustomer">The updated customer information.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the update operation.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] CustomerModel updatedCustomer)
    {
        try
        {
            var validationResult = _validator.Validate(updatedCustomer);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
                throw new ValidationException(errors);
            }

            var customer = await _customerRepository.UpdateAsync(id, updatedCustomer) ?? throw new NotFoundException(nameof(CustomerModel), id);

            return NoContent();
        }
        catch (Exception ex)
        {
            return HandleException(ex, _logger, nameof(UpdateCustomer));
        }
    }

    /// <summary>
    /// Retrieves a list of all customers.
    /// </summary>
    /// <returns>An IActionResult containing the list of customers.</returns>
    [HttpGet]
    public async Task<IActionResult> GetCustomers()
    {
        try
        {
            return Ok(await _customerRepository.GetAllAsync());
        }
        catch (Exception ex)
        {
            return HandleException(ex, _logger, nameof(GetCustomers));
        }
    }

    /// <summary>
    /// Retrieves a specific customer by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the customer to retrieve.</param>
    /// <returns>An IActionResult containing the customer data if found, or a NotFound result if not found.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomer(Guid id)
    {
        try
        {
            var customer = await _customerRepository.GetByIdAsync(id) ?? throw new NotFoundException(nameof(CustomerModel), id);
            return Ok(customer);
        }
        catch (Exception ex)
        {
            return HandleException(ex, _logger, nameof(GetCustomer));
        }
    }

    /// <summary>
    /// Deletes a customer by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the customer to delete.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the delete operation.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(Guid id)
    {
        try
        {
            return await _customerRepository.DeleteAsync(id)
                ? NoContent()
                : throw new NotFoundException(nameof(CustomerModel), id);
        }
        catch (Exception ex)
        {
            return HandleException(ex, _logger, nameof(DeleteCustomer));
        }
    }
}
