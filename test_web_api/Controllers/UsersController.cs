using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using test_web_api.Data;
using test_web_api.Dtos;
using test_web_api.Models;
using test_web_api.Services;

namespace test_web_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController:ControllerBase
    {
        //private readonly MyDBContext _context;
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/users
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetAllAsync();//_context.Users.ToListAsync();
            return Ok(users);
        }

        // GET: api/users/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _userService.GetByIdAsync(id);//_context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // POST: api/users
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserDto userDto)
        {
            try
            {
                var user = await _userService.CreateAsync(userDto);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }            
        }

        // PUT: api/users/1
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDto userDto)
        {
            var user = _userService.UpdateAsync(id, userDto);
            return user==null? NotFound() : Ok(user);
        }

        // DELETE: api/users/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var response = await _userService.DeleteAsync(id);
            return response ? NoContent() : NotFound();
        }
    }
}
