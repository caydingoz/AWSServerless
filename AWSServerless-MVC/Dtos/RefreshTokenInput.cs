namespace AWSServerless_MVC.Dtos
{
	public class RefreshTokenInput
	{
		public string AccessToken { get; set; } = string.Empty;
		public string RefreshToken { get; set; } = string.Empty;
	}
}