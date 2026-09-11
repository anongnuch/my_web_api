using test_web_api_mongo.Dtos;
using test_web_api_mongo.Models;
using test_web_api_mongo.Repositories;

namespace test_web_api_mongo.Services
{
    public class UserService:IUserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<User>> GetAllAsync() => await _repo.GetAllAsync();

        public async Task<User?> GetByIdAsync(string id) => await _repo.GetByIdAsync(id);

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

            await _repo.CreateAsync(user);
            return user;
        }

        public async Task<User?> UpdateAsync(string id, UserDto dto)
        {
            var user = new User
            {
                Id = id,
                Username = dto.Username,
                Password = dto.Password,
                FullName = dto.FullName,
                Email = dto.Email
            };

            await _repo.UpdateAsync(id,user);
            return user;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            await _repo.DeleteAsync(id);
            return true;
        }
    }
}
