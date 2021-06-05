using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyMonolithicApp.Domain.Auth;
using MyMonolithicApp.Domain.Users;

namespace MyMonolithicApp.Infrastructure.Auth
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtIssuerOptions _jwtOptions;
        private readonly SecurityTokenHandler _tokenHandler;

        public JwtTokenGenerator(IOptions<JwtIssuerOptions> jwtOptions, SecurityTokenHandler tokenHandler)
        {
            _jwtOptions = jwtOptions.Value;
            _tokenHandler = tokenHandler;
        }

        public string CreateToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, JwtIssuerOptions.JtiGenerator()),
                new Claim(JwtRegisteredClaimNames.Iat,
                    new DateTimeOffset(JwtIssuerOptions.IssuedAt).ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            };
            var jwt = new JwtSecurityToken(
                _jwtOptions.Issuer,
                _jwtOptions.Audience,
                claims,
                JwtIssuerOptions.NotBefore,
                _jwtOptions.Expiration,
                _jwtOptions.SigningCredentials);

            var encodedJwt = _tokenHandler.WriteToken(jwt);
            return encodedJwt;
        }
    }
}
