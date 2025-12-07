using CustomersApi.Models;

namespace CustomersApi.Data.Repositories;

public interface ICustomerRepository
{
    Task<CustomerModel> AddAsync(CustomerModel customer);
    Task<CustomerModel?> UpdateAsync(Guid id, CustomerModel customer);
    Task<IEnumerable<CustomerModel>> GetAllAsync();
    Task<CustomerModel?> GetByIdAsync(Guid id);
    Task<bool> DeleteAsync(Guid id);
}