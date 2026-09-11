using MongoDB.Driver;
using test_web_api_mongo.Dtos;
using test_web_api_mongo.Models;

namespace test_web_api_mongo.Repositories
{
    public class ProductRepository:IProductRepository
    {
        private readonly IMongoCollection<Product> _collection;

        public ProductRepository(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("MongoDb"));
            var database = client.GetDatabase("MyDB");
            _collection = database.GetCollection<Product>("Products");
        }

        public async Task<List<Product>> GetAllAsync() =>
            await _collection.Find(_ => true).ToListAsync();

        public async Task<Product?> GetByIdAsync(string id) =>
            await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Product product) =>
            await _collection.InsertOneAsync(product);

        public async Task UpdateAsync(string id, Product product) =>
            await _collection.ReplaceOneAsync(p => p.Id == id, product);

        public async Task DeleteAsync(string id) =>
            await _collection.DeleteOneAsync(p => p.Id == id);

        public async Task<bool> ProductExistAsync(string productName)
        {
            var count = await _collection.CountDocumentsAsync(p => p.Name == productName);
            return count > 0;
        }
    }
}
