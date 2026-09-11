using test_web_api.Models;

namespace test_web_api.Repositories
{
    public interface IProductPriceRepository
    {
        Task<ProductPrice?> GetByIdAsync(int id);
        Task DeleteAsync(ProductPrice productPrice);
        Task DeleteRangeAsync(IEnumerable<ProductPrice> prices);
    }
}
