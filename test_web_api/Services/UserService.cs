using test_web_api.Dtos;
using test_web_api.Models;
using test_web_api.Repositories;

namespace test_web_api.Services
{
    public class UserService:IUserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<User>> GetAllAsync() => await _repo.GetAllAsync();

        public async Task<User?> GetByIdAsync(int id) => await _repo.GetByIdAsync(id);

        public async Task<User> CreateAsync(UserDto dto)
        {
            if (await _repo.UsernameExistsAsync(dto.Username))
                throw new Exception("Username already exists");

            var user = new User
            {
                Username = dto.Username,
                Password = dto.Password,
                FullName = dto.FullName,
                Email = dto.Email,
                CreatedDate = DateTime.Now
            };

            await _repo.AddAsync(user);
            return user;
        }

        public async Task<User?> UpdateAsync(int id, UserDto dto)
        {
            var user = await _repo.GetByIdAsync(id);
            if (user == null) return null;

            user.Username = dto.Username;
            user.Password = dto.Password;
            user.FullName = dto.FullName;
            user.Email = dto.Email;

            await _repo.UpdateAsync(user);
            return user;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _repo.GetByIdAsync(id);
            if (user == null) return false;

            await _repo.DeleteAsync(user);
            return true;
        }
    }
}
