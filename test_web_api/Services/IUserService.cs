using test_web_api.Dtos;
using test_web_api.Models;

namespace test_web_api.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
        Task<User> CreateAsync(UserDto dto);
        Task<User?> UpdateAsync(int id, UserDto dto);
        Task<bool> DeleteAsync(int id);

    }
}
