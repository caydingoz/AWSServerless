using AWSServerless_MVC.Entities;
using AWSServerless_MVC.Interfaces.Repositories;
using AWSServerless_MVC.Interfaces.Services;
using AWSServerless_MVC.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AWSServerless_MVC.Services
{
    public class TokenHandlerService : ITokenHandlerService
    {
        AuthConfiguration AuthConfiguration { get; set; }
        IUserRefreshTokenRepository UserRefreshTokenRepository { get; set; }

        public TokenHandlerService(IServiceProvider serviceProvider)
        {
            AuthConfiguration = serviceProvider.GetService<AuthConfiguration>() ?? throw new ArgumentNullException();
            UserRefreshTokenRepository = serviceProvider.GetService<IUserRefreshTokenRepository>() ?? throw new ArgumentNullException();
        }
        public async Task<JwtToken> CreateTokenAsync(ApplicationUser user, IList<string>? userRoles)
        {
            if(userRoles is null)
                userRoles = new List<string>();

            JwtSecurityToken token;
            string refreshToken;
            (token, refreshToken) = GenerateAccessTokenAndRefreshToken(user, userRoles.ToList());

            var tokenResult = new JwtToken
            {
                ExpiresIn = (long)token.ValidTo.Subtract(DateTime.UtcNow).TotalSeconds,
                RefreshToken = refreshToken,
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            };

            await UserRefreshTokenRepository.InsertOneAsync(new UserRefreshToken
            {
                RefreshToken = refreshToken,
                AccessToken = tokenResult.AccessToken,
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(AuthConfiguration.JWT.RefreshTokenValidityInDays),
                UserId = new Guid(user.Id)
            });

            return tokenResult;
        }

        private Tuple<JwtSecurityToken, string> GenerateAccessTokenAndRefreshToken(ApplicationUser user, List<string> userRoles)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Email, user.Email),
            };

            if(userRoles.Any())
                userRoles.ForEach(userRole => authClaims.Add(new Claim(ClaimTypes.Role, userRole)));

            return new Tuple<JwtSecurityToken, string>(GenerateAccessToken(authClaims), GenerateRefreshToken());
        }

        private JwtSecurityToken GenerateAccessToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AuthConfiguration.JWT.Secret));

            var token = new JwtSecurityToken(
                issuer: AuthConfiguration.JWT.ValidIssuer,
                audience: AuthConfiguration.JWT.ValidAudience,
                expires: DateTime.UtcNow.AddMinutes(AuthConfiguration.JWT.TokenValidityInMinutes),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );
            return token;
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public IEnumerable<Claim> GetPrincipalFromExpiredToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            return jwtToken.Claims;
        }
    }
}
