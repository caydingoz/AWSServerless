using Patika.Framework.Shared.Entities;

namespace AWSServerless1.Models
{
    public class User : GenericEntity<Guid>
    {
        public string Name { get; set; } =  string.Empty;
    }
}
