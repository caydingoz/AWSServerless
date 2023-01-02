using AWSServerless_MVC.Interfaces.Repositories;
using AWSServerless_MVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace AWSServerless_MVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository Repo;
        public UserController(IUserRepository repo)
        {
            Repo = repo;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetById(Guid userId)
        {
            var user = await Repo.GetByIdAsync(userId);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await Repo.GetAllAsync();
            return Ok(users);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(string name)
        {
            return Ok(await Repo.InsertOneAsync(new User { Name = name }));
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            var user = await Repo.GetByIdAsync(userId);
            if (user == null) return NotFound();
            await Repo.DeleteOneAsync(user);
            return NoContent();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser(User user)
        {
            if (!await Repo.AnyAsync(x => x.Id == user.Id)) return NotFound();
            await Repo.UpdateOneAsync(user);
            return Ok(user);
        }
    }
}
