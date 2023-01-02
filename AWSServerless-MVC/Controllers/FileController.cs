using AWSServerless_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Patika.Framework.Utilities.Excel.Extensions;

namespace AWSServerless_MVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        [HttpPost]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        public async Task<IActionResult> ImportExcelFileAsync(IFormFile file)
        {
            var data = file.MapExcelToEnumarable<ExampleExcel>();
            return Ok(data);
        }
    }
}
