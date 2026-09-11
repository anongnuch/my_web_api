using test_web_api_mongo.Models;

namespace test_web_api_mongo.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(string id);
        Task CreateAsync(User user);
        Task UpdateAsync(string id, User user);
        Task DeleteAsync(string id);
        Task<bool> UsernameExistsAsync(string username);
    }
}
