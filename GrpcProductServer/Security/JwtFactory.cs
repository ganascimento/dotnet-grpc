using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GrpcProductServer.Entities;
using Microsoft.IdentityModel.Tokens;

namespace GrpcProductServer.Security;

public class JwtFactory
{
    private readonly IConfiguration _configuration;

    public JwtFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateJwtToken(UserEntity user)
    {
        var claims = new List<Claim>();
        var secret = _configuration.GetValue<string>("JWT:Secret")!;
        var issuer = _configuration.GetValue<string>("JWT:Issuer")!;
        var audience = _configuration.GetValue<string>("JWT:Audience")!;
        var expiration = _configuration.GetValue<double>("JWT:Expiration")!;

        claims.Add(new Claim(ClaimTypes.UserData, ((int)user.Id!).ToString()));

        var identityClaims = new ClaimsIdentity();
        identityClaims.AddClaims(claims);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(secret);
        var expireDate = DateTime.UtcNow.AddHours(expiration);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = identityClaims,
            Issuer = issuer,
            Audience = audience,
            Expires = expireDate,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
    }
}