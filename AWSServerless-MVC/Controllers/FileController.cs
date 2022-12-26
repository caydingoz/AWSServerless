using AWSServerless_MVC.Interfaces.Repositories;
using AWSServerless_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Patika.Framework.Utilities.Excel.Extensions;

namespace AWSServerless_MVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IUserRepository Repo;
        public FileController(IUserRepository repo)
        {
            Repo = repo;
        }
        [HttpPost]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        public async Task<IActionResult> Createuser(IFormFile file)
        {
            var data = file.MapExcelToEnumarable<ExampleExcel>();
            return Ok(data);
        }
    }
}
