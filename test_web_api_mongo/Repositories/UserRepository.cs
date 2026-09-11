//using Microsoft.EntityFrameworkCore;
//using test_web_api_mongo.Data;
//using test_web_api_mongo.Models;

using MongoDB.Driver;
using test_web_api_mongo.Models;

namespace test_web_api_mongo.Repositories
{
    public class UserRepository:IUserRepository
    {
        private readonly IMongoCollection<User> _collection;

        public UserRepository(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("MongoDb"));
            var database = client.GetDatabase("MyDB");
            _collection = database.GetCollection<User>("Users");
        }

        public async Task<IEnumerable<User>> GetAllAsync() => await _collection.Find(_ => true).ToListAsync();

        public async Task<User?> GetByIdAsync(string id) => await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();      

        public async Task CreateAsync(User user) {            
            await _collection.InsertOneAsync(user);
        } 

        public async Task UpdateAsync(string id, User user) =>
            await _collection.ReplaceOneAsync(p => p.Id == id, user);

        public async Task DeleteAsync(string id) =>
            await _collection.DeleteOneAsync(p => p.Id == id);

        public async Task<bool> UsernameExistsAsync(string username)
        {
            var count = await _collection.CountDocumentsAsync(u => u.Username == username);
            return count > 0;
        }

    }
}
