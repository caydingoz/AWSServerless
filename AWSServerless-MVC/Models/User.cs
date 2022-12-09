using Patika.Framework.Shared.Entities;

namespace AWSServerless_MVC.Models
{
    public class User : GenericEntity<Guid>
    {
        public string Name { get; set; } =  string.Empty;
    }
}
