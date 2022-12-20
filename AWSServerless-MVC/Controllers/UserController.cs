﻿using AWSServerless_MVC.Interfaces.Repositories;
using AWSServerless_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

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
        [HttpGet("Deneme")]
        public async Task<IActionResult> Deneme()
        {

            var connectionStringBuilder = new MySqlConnectionStringBuilder
            {
                Server = "cemil-clean-db.crrusvpzju7v.eu-west-1.rds.amazonaws.com",
                UserID = "admin",
                Password = "12345678",
                Database = "Cemil",
                Port = 3306
            };

            using (var conn = new MySqlConnection(connectionStringBuilder.ToString()))
            {
                conn.Open();

            }
            return Ok();
        }
        [HttpGet("Denemee")]
        public async Task<IActionResult> Denemee()
        {
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
