using AWSServerless1.Interfaces.Repositories;
using AWSServerless1.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace AWSServerless1.Controllers
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
        [HttpGet("error")]
        public IActionResult Error()
        {
            return BadRequest("Jitterbit Esra");
        }
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetById(Guid userId)
        {
            var user = await Repo.GetByIdAsync(userId);
            if (user == null) return NotFound();
            return Ok(user);
        }
        [HttpGet("Deneme")]
        public async Task<IActionResult> Deneme()
        {

            var connectionStringBuilder = new MySqlConnectionStringBuilder
            {
                Server = "test-db.crrusvpzju7v.eu-west-1.rds.amazonaws.com",
                UserID = "admin",
                Password = "12345678",
                Database = "TestDB",
                Port = 3306
            };

            using (var conn = new MySqlConnection(connectionStringBuilder.ToString()))
            {
                conn.Open();

            }
            return Ok(await (new HttpClient()).GetStringAsync("https://catfact.ninja/fact"));
        }
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await Repo.GetAllAsync();
            return Ok(users);
        }
        [HttpPost]
        public async Task<IActionResult> Createuser(string name)
        {
            return Ok(await Repo.InsertOneAsync(new User { Name = name }));
        }
        [HttpDelete("{userId}")]
        public async Task<IActionResult> Deleteuser(Guid userId)
        {
            var user = await Repo.GetByIdAsync(userId);
            if (user == null) return NotFound();
            await Repo.DeleteOneAsync(user);
            return NoContent();
        }
        [HttpPut]
        public async Task<IActionResult> Updateuser(User user)
        {
            if (!await Repo.AnyAsync(x => x.Id == user.Id)) return NotFound();
            await Repo.UpdateOneAsync(user);
            return Ok(user);
        }
    }
}
