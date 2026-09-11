using test_web_api_mongo.Dtos;
using test_web_api_mongo.Models;

namespace test_web_api_mongo.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(string id);
        Task<User> CreateAsync(UserDto dto);
        Task<User?> UpdateAsync(string id, UserDto dto);
        Task<bool> DeleteAsync(string id);

    }
}
