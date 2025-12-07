using CustomersApi.Models;

namespace CustomersApi.Data.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private static List<CustomerModel> _customers =
    [
        new CustomerModel { Id = Guid.Parse("bafda49f-f76a-4328-8c2f-c637d6e74e85"), Name = "John Doe", Email = "john.doe@example.com", Address = "456 Elm St, Anytown, USA" },
        new CustomerModel { Id = Guid.Parse("ba4adcc3-adfc-443d-b760-01c5051dc4f1"), Name = "Jane Smith", Email = "jane.smith@example.com", Address = "123 Main St, Anytown, USA" },
        new CustomerModel { Id = Guid.Parse("f5fba9a0-0745-4028-93f1-9053f5031b10"), Name = "Alice Johnson", Email = "alice.johnson@example.com", Address = "789 Oak St, Anytown, USA" }
    ];

    public Task<CustomerModel> AddAsync(CustomerModel customer)
    {
        _customers.Add(customer);
        return Task.FromResult(customer);
    }

    public async Task<CustomerModel?> UpdateAsync(Guid id, CustomerModel customer)
    {
        var existingCustomer = await GetByIdAsync(id);
        if (existingCustomer == null)
        {
            return null;
        }

        existingCustomer.Name = customer.Name;
        existingCustomer.Email = customer.Email;
        existingCustomer.Address = customer.Address;

        return existingCustomer;
    }

    public Task<IEnumerable<CustomerModel>> GetAllAsync() => Task.FromResult<IEnumerable<CustomerModel>>(_customers);

    public Task<CustomerModel?> GetByIdAsync(Guid id) => Task.FromResult<CustomerModel?>(_customers.FirstOrDefault(c => c.Id == id));

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existingCustomer = await GetByIdAsync(id);
        if (existingCustomer == null)
        {
            return false;
        }

        _customers.Remove(existingCustomer);
        return true;
    }
}
