using Microsoft.EntityFrameworkCore;
using test_web_api.Data;
using test_web_api.Dtos;
using test_web_api.Models;
using test_web_api.Repositories;

namespace test_web_api.Services
{
    public class ProductService:IProductService
    {
        private readonly IProductRepository _productRepo;
        private readonly IProductPriceRepository _productPriceRepo;
        private readonly MyDBContext _context;

        public ProductService(IProductRepository productRepo, IProductPriceRepository productPriceRepo, MyDBContext context)
        {
            _productRepo = productRepo;
            _productPriceRepo = productPriceRepo;
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync() { 
            var products = await _productRepo.GetAllAsync();
            return products;
        }       

        public async Task<Product?> GetByIdAsync(int id) => await _productRepo.GetByIdAsync(id);

        public async Task<Product> CreateAsync(ProductCreateDto dto)
        {
            if (await _productRepo.ProductExistsAsync(dto.Name))
                throw new Exception("Product already exists");

            var product = new Product
            {
                Name = dto.Name,
                CreatedDate = DateTime.Now,
            };

            if (dto.ProductPrices != null)
            { 
                product.ProductPrices = dto.ProductPrices.Select(x  => new ProductPrice { 
                    Price = x.Price,
                    EffectiveDate = x.EffectiveDate,
                }).ToList();
            }

            await _productRepo.AddAsync(product);
            return product;
        }

        public async Task<Product?> UpdateAsync(int id, ProductUpdateDto dto)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            var product = await _productRepo.GetByIdAsync(id);
            if (product == null) return null;
            try
            {
                product.Name = dto.Name;
                // --- จัดการ ProductPrices ---
                var oldPrices = product.ProductPrices.ToList();
                
                // 1. ลบรายการที่ไม่มีใน DTO แล้ว
                foreach (var oldPrice in oldPrices)
                {
                    if (!dto.ProductPrices.Any(p => p.Id == oldPrice.Id))
                    {
                        await _productPriceRepo.DeleteAsync(oldPrice);
                    }
                }

                // 2. เพิ่มรายการใหม่
                foreach (var newPrice in dto.ProductPrices.Where(p => p.Id == null))
                {
                    product.ProductPrices.Add(new ProductPrice
                    {
                        Price = newPrice.Price,
                        EffectiveDate = newPrice.EffectiveDate,
                        ProductId = product.Id
                    });
                }

                // 3. อัปเดตรายการที่เหลือ
                foreach (var updateDto in dto.ProductPrices.Where(p => p.Id != null))
                {
                    var target = product.ProductPrices.FirstOrDefault(p => p.Id == updateDto.Id);
                    if (target != null)
                    {
                        target.Price = updateDto.Price;
                        target.EffectiveDate = updateDto.EffectiveDate;
                    }
                }

                await _productRepo.UpdateAsync(product);
                transaction.Commit();
                return product;
            }
            catch (Exception)
            {
                transaction.Rollback();
                return null;              
            }
            
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product == null) return false;

            await _productPriceRepo.DeleteRangeAsync(product.ProductPrices);

            await _productRepo.DeleteAsync(product);
            return true;
        }

    }
}
