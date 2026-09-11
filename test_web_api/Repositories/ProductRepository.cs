using Microsoft.EntityFrameworkCore;
using test_web_api.Data;
using test_web_api.Models;

namespace test_web_api.Repositories
{
    public class ProductRepository:IProductRepository
    {
        private MyDBContext _context;
        public ProductRepository(MyDBContext context) { 
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync() {
            var products = await _context.Products
             .Include(p => p.ProductPrices)
             .ToListAsync();
            return products;
        }


        public async Task<Product?> GetByIdAsync(int id) {
            try
            {
                var products= await _context.Products.FindAsync(id);
            if (products != null)
            {
                await _context.Entry(products).Collection(p => p.ProductPrices).LoadAsync();
            }
            return products;
            } 
            catch(Exception e)
            {
                throw e;
            }
            
        } 

        public async Task AddAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Product product)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ProductExistsAsync(string productName)
        {
            return await _context.Products.AnyAsync(u => u.Name == productName);
        }
    }
}
