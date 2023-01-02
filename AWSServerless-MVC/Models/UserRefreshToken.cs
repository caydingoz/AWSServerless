using Patika.Framework.Shared.Entities;

namespace AWSServerless_MVC.Models;
public class UserRefreshToken : GenericEntity<Guid>
{
    public Guid UserId { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiryTime { get; set; }
}
