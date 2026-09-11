using test_web_api_mongo.Dtos;
using test_web_api_mongo.Models;
using test_web_api_mongo.Repositories;

namespace test_web_api_mongo.Services
{
    public class ProductService:IProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            var products = await _repository.GetAllAsync();
            if (products == null) return new List<Product>();
            //return products.Select(p => new Product
            //{
            //    Id = p.Id,
            //    Name = p.Name,
            //    Prices = p.Prices.Select(pp => new ProductPrice
            //    {
            //        Currency = pp.Currency,
            //        Amount = pp.Amount
            //    }).ToList()
            //}).ToList();
            return products;
        }

        public async Task<Product?> GetByIdAsync(string id)
        {
            var p = await _repository.GetByIdAsync(id);
            if (p == null) return null;

            //return new Models.Product
            //{
            //    Id = p.Id,
            //    Name = p.Name,
            //    Prices = p.Prices.Select(pp => new ProductPrice
            //    {
            //        Currency = pp.Currency,
            //        Amount = pp.Amount
            //    }).ToList()
            //};
            return p;
        }

        public async Task<Product?> CreateAsync(ProductCreateDto dto)
        {
            var result = await _repository.ProductExistAsync(dto.Name);
            if (result)  
                throw new Exception("Product already exists");

            var product = new Models.Product
            {
                Name = dto.Name,
                Prices = dto.Prices.Select(pp => new ProductPrice
                {
                    Currency = pp.Currency,
                    Amount = pp.Amount
                }).ToList()
            };
            await _repository.CreateAsync(product);
            return product;
        }

        public async Task<Product?> UpdateAsync(string id, ProductUpdateDto dto)
        {
            var product = new Product
            {
                Id = id,
                Name = dto.Name,
                Prices = dto.Prices.Select(pp => new ProductPrice
                {
                    Currency = pp.Currency,
                    Amount = pp.Amount
                }).ToList()
            };
            await _repository.UpdateAsync(id,product);
            return product; 
        }

        public async Task<bool> DeleteAsync(string id) {
            await _repository.DeleteAsync(id);
            return true;
        }
           

    }
}
