using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using cinema.Models;
using FluentResults;
using Microsoft.IdentityModel.Tokens;

namespace cinema.Helpers;

public class JWTGenerator(IConfiguration _configuration) : IJWTGenerator
{
    public static readonly string WS_IDENTITY_CLAIM = "ws_identity";
    public static readonly int REFRESH_INTERVAL_MINS = 300;
    public static readonly string JWT_ENCRYPTION_KEY_ID = "JWT_KEY";

    public IResult<string> Generate(Player user)
    {
        var encryptKeyBytes = Encoding.ASCII.GetBytes(_configuration[JWT_ENCRYPTION_KEY_ID] ?? throw new ArgumentNullException(JWT_ENCRYPTION_KEY_ID));
        if (encryptKeyBytes is null)
        {
            return Result.Fail<string>("UNABLE TO GENERATE AUTHENTICATION TOKEN");
        }

        var claims = new List<Claim>
            {
                new(WS_IDENTITY_CLAIM, JsonSerializer.Serialize(user)),
            };

        var tokenConfig = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(REFRESH_INTERVAL_MINS),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(encryptKeyBytes), SecurityAlgorithms.HmacSha256),
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenConfig);
        if (token == null)
        {
            return Result.Fail<string>("UNABLE TO GENERATE AUTHENTICATION TOKEN");
        }

        return Result.Ok(tokenHandler.WriteToken(token)!);
    }

    public IResult<ClaimsPrincipal> ValidateAndDecrypt(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration[JWT_ENCRYPTION_KEY_ID] ?? throw new ArgumentNullException(JWT_ENCRYPTION_KEY_ID));
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            return Result.Ok(principal);
        }
        catch (Exception ex)
        {
            return Result.Fail<ClaimsPrincipal>("Token validation failed: " + ex.Message);
        }
    }
}