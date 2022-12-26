using Npoi.Mapper.Attributes;
using Patika.Framework.Utilities.Excel.Interfaces;

namespace AWSServerless_MVC.Models
{
    public class ExampleExcel : IExportable
    {
        [Column("Ad")]
        public string Name { get; set; } = string.Empty;

        [Column("Yaş")]
        public int Age { get; set; }
    }
}
