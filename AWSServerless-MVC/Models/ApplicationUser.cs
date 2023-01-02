using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AWSServerless_MVC.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;
        [StringLength(100)]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime? Updated { get; set; }
        public DateTime? LastLogin { get; set; }
        public string GetFullName() => $"{FirstName} {LastName}";
        public Guid? GetId() => string.IsNullOrEmpty(Id) ? null : new Guid(Id);
    }
}
