using CatalogApi.Models;

namespace CatalogApi.Data.Repositories;

public interface IProductRepository
{
    public Task<ProductModel> AddAsync(ProductModel product);
    public Task<ProductModel?> UpdateAsync(Guid id, ProductModel product);
    public Task<IEnumerable<ProductModel>> GetAllAsync();
    public Task<ProductModel?> GetByIdAsync(Guid id);
    public Task<bool> DeleteAsync(Guid id);
}