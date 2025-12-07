using CustomersApi.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OnlineStoreMVP.ServiceDefaults.Controllers;
using OnlineStoreMVP.ServiceDefaults.Exceptions;

namespace CustomersApi.Controllers;

[Route("api/[controller]")]
public class CustomersController(ILogger<CustomersController> logger, IValidator<CustomerModel> validator) : BaseController
{
    private readonly ILogger<CustomersController> _logger = logger;
    private readonly IValidator<CustomerModel> _validator = validator;

    private static List<CustomerModel> _customers =
    [
        new CustomerModel { Id = Guid.NewGuid(), Name = "John Doe", Email = "john.doe@example.com", Address = "123 Main St, Anytown, USA" },
        new CustomerModel { Id = Guid.NewGuid(), Name = "Jane Smith", Email = "jane.smith@example.com", Address = "456 Elm St, Othertown, USA" },
        new CustomerModel { Id = Guid.NewGuid(), Name = "Bob Johnson", Email = "bob.johnson@example.com", Address = "789 Oak St, Sometown, USA" }
    ];

    /// <summary>
    /// Retrieves a list of all customers.
    /// </summary>
    /// <returns>An IActionResult containing the list of customers.</returns>
    [HttpGet]
    public IActionResult GetCustomers()
    {
        try
        {
            return Ok(_customers);
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
    public IActionResult GetCustomer(Guid id)
    {
        try
        {
            // business logic to get customer by id
            var customer = _customers.FirstOrDefault(c => c.Id == id);
            if (customer == null)
            {
                return NotFound();
            }
            return Ok(customer);
        }
        catch (Exception ex)
        {
            return HandleException(ex, _logger, nameof(GetCustomer));
        }
    }

    /// <summary>
    /// Creates a new customer record using the provided customer information.
    /// </summary>
    /// <param name="customer">The customer data to create. Must not be null and must satisfy all validation requirements.</param>
    /// <returns>An IActionResult that represents the result of the create operation. Returns a 201 Created response with the
    /// created customer if successful, or an error response if validation fails or an exception occurs.
    /// </returns>
    [HttpPost]
    public IActionResult CreateCustomer([FromBody] CustomerModel customer)
    {
        try
        {
            // Validate the incoming customer data
            var validationResult = _validator.Validate(customer);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
                throw new OnlineStoreMVP.ServiceDefaults.Exceptions.ValidationException(errors);
            }

            // business logic to add customer
            customer.Id = Guid.NewGuid();
            _customers.Add(customer);

            return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
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
    /// <param name="customer">The updated customer information.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the update operation.</returns>
    [HttpPut("{id}")]
    public IActionResult UpdateCustomer(Guid id, [FromBody] CustomerModel customer)
    {
        try
        {
            // business logic to find existing customer
            var existingCustomer = _customers.FirstOrDefault(c => c.Id == id) ?? throw new NotFoundException(nameof(CustomerModel), id);

            // Validate the incoming customer data
            var validationResult = _validator.Validate(customer);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
                throw new OnlineStoreMVP.ServiceDefaults.Exceptions.ValidationException(errors);
            }

            // business logic to update customer
            existingCustomer.Name = customer.Name;
            existingCustomer.Email = customer.Email;
            existingCustomer.Address = customer.Address;
            
            return NoContent();
        }
        catch (Exception ex)
        {
            return HandleException(ex, _logger, nameof(UpdateCustomer));
        }
    }

    /// <summary>
    /// Deletes a customer by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the customer to delete.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the delete operation.</returns>
    [HttpDelete("{id}")]
    public IActionResult DeleteCustomer(Guid id)
    {
        try
        {
            // business logic to find existing customer
            var customer = _customers.FirstOrDefault(c => c.Id == id) ?? throw new NotFoundException(nameof(CustomerModel), id);
            
            // business logic to delete customer
            _customers.Remove(customer);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            return HandleException(ex, _logger, nameof(DeleteCustomer));
        }
    }
}
