using CatalogApi.Models;
using OnlineStoreMVP.ServiceDefaults.Common.Exceptions;

namespace CatalogApi.Data.Repositories;

public class ProductRepository : IProductRepository
{
    private static List<ProductModel> _products = [
        new ProductModel { Id = Guid.Parse("6dbad659-57f9-4639-b7b6-d7ef1c75321a"), Name = "Laptop", Description = "A high-performance laptop.", Price = 999.99M, Stock = 10 },
        new ProductModel { Id = Guid.Parse("05ac7e30-a71c-4cf5-b7c1-01507aa70a31"), Name = "Smartphone", Description = "A latest model smartphone.", Price = 699.99M, Stock = 100 },
        new ProductModel { Id = Guid.Parse("cbbb4fb6-1dbe-4e58-9fa0-f693b2c77229"), Name = "Headphones", Description = "Noise-cancelling headphones.", Price = 199.99M, Stock = 50 }
    ];

    public Task<ProductModel> AddAsync(ProductModel product)
    {
        product.Id = Guid.NewGuid();
        _products.Add(product);
        return Task.FromResult(product);
    }

    public async Task<ProductModel?> UpdateAsync(Guid id, ProductModel product)
    {
        var existingProduct = await GetByIdAsync(id);
        if (existingProduct is null)
        {
            return null;
        }

        existingProduct.Name = product.Name;
        existingProduct.Price = product.Price;
        existingProduct.Description = product.Description;
        existingProduct.Stock = product.Stock;

        _products[_products.IndexOf(existingProduct)] = existingProduct;

        return existingProduct;
    }

    public Task<IEnumerable<ProductModel>> GetAllAsync() => Task.FromResult<IEnumerable<ProductModel>>(_products);

    public Task<ProductModel?> GetByIdAsync(Guid id) => Task.FromResult<ProductModel?>(_products.FirstOrDefault(p => p.Id == id));

    public async Task<bool> DeleteAsync(Guid id)
    {
        var product = await GetByIdAsync(id);
        if (product == null)
        {
            return false;
        }

        _products.Remove(product);
        return true;
    }
}
