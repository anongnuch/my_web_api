using test_web_api_mongo.Dtos;
using test_web_api_mongo.Models;

namespace test_web_api_mongo.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(string id);
        Task<Product?> CreateAsync(ProductCreateDto dto);
        Task<Product?> UpdateAsync(string id, ProductUpdateDto dto);
        Task<bool> DeleteAsync(string id);
    }
}
