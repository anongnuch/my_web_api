using test_web_api_mongo.Dtos;
using test_web_api_mongo.Models;

namespace test_web_api_mongo.Repositories
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(string id);
        Task CreateAsync(Product product);
        Task UpdateAsync(string id, Product product);
        Task DeleteAsync(string id);
        Task<bool> ProductExistAsync(string productName);
    }
}
