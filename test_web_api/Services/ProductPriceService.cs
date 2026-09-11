using Microsoft.EntityFrameworkCore;
using test_web_api.Models;
using test_web_api.Repositories;

namespace test_web_api.Services
{
    public class ProductPriceService:IProductPriceService
    {
        private readonly IProductPriceRepository _productPriceRepo;
        public ProductPriceService(IProductPriceRepository productPriceRepo) 
        {
            _productPriceRepo=productPriceRepo;
        }

        //public async Task<bool> DeleteAsync(int id)
        //{
        //    var productPrice = await _productPriceRepo.GetByIdAsync(id);
        //    if (productPrice == null) return false;

        //    await _productPriceRepo.DeleteAsync(productPrice);
        //    return true;
        //}

        //public async Task<bool> DeleteRangeAsync(IEnumerable<ProductPrice> prices)
        //{
        //    if (prices == null) return false;
        //    await _productPriceRepo.DeleteRangeAsync(prices);
        //    return true;
        //}
       
    }
}
