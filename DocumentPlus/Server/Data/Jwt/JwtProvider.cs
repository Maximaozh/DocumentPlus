using DocumentPlus.Shared.Dto.Users;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DocumentPlus.Server.Data.Jwt;

public class JwtProvider
{
    private readonly IConfiguration configuration;
    public JwtProvider(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public string GenerateJWT(UserInfo data)
    {
        List<Claim> claims = new List<Claim>() {
            new Claim("UserId", data.Id),
            new Claim(ClaimTypes.NameIdentifier, data.Login),
            new Claim(ClaimTypes.Role, data.Role),
            new Claim("Name", data.Name),
            new Claim("Surname", data.Surname)
        };

        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("JwtSettings").GetValue<string>("Serial")));

        SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new JwtSecurityToken(
            claims: claims.ToArray(),
            signingCredentials: creds
            );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
