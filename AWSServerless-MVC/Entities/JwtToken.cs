namespace AWSServerless_MVC.Entities
{
    public class JwtToken
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = string.Empty;
        public long ExpiresIn { get; set; }
    }
}
