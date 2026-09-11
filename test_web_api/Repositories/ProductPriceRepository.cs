using test_web_api.Data;
using test_web_api.Models;

namespace test_web_api.Repositories
{
    public class ProductPriceRepository: IProductPriceRepository
    {
        private readonly MyDBContext _context;
        public ProductPriceRepository(MyDBContext Context)
        { 
            _context= Context;
        }

        public async Task<ProductPrice?> GetByIdAsync(int id) => await _context.ProductPrices.FindAsync(id);

        public async Task DeleteAsync(ProductPrice productPrice)
        {
            _context.ProductPrices.Remove(productPrice);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRangeAsync(IEnumerable<ProductPrice> prices)
        {
            _context.ProductPrices.RemoveRange(prices);
            await _context.SaveChangesAsync();
        }
    }
}
