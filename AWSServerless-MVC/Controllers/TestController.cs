using AWSServerless_MVC.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace AWSServerless_MVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("db")]
        public async Task<IActionResult> DBTestAsync()
        {

            var connectionStringBuilder = new MySqlConnectionStringBuilder
            {
                Server = "cemil-public.crrusvpzju7v.eu-west-1.rds.amazonaws.com",
                UserID = "admin",
                Password = "12345678",
                Database = "Cemil",
                Port = 3306
            };

            using (var conn = new MySqlConnection(connectionStringBuilder.ToString()))
            {
                await conn.OpenAsync();

            }
            return Ok("Database ok");
        }
        [HttpGet("int")]
        public async Task<IActionResult> InternetTestAsync()
        {
            return Ok(await new HttpClient().GetStringAsync("https://catfact.ninja/fact"));
        }
        [HttpGet("auth")]
        [Authorize(Roles = RoleConsts.USER_ROLE)]
        public IActionResult AuthenticationTest()
        {
            return Ok("Authorize ok");
        }
    }
}
