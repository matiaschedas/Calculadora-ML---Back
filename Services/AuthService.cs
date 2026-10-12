using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Matoli.Api.Models;

namespace Matoli.Api.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;

    public AuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool ValidateCredentials(string username, string password)
    {
        var configUser = _configuration["AuthUser:Username"] ?? "admin";
        var storedHash = _configuration["AuthUser:PasswordHash"] ?? "";

        // Si no hay hash configurado, falla por seguridad
        if (string.IsNullOrEmpty(storedHash)) return false;

        return string.Equals(username?.Trim(), configUser.Trim(), StringComparison.OrdinalIgnoreCase) &&
               BCrypt.Net.BCrypt.Verify(password, storedHash);
    }

    public LoginResponse GenerateToken(string username)
    {
        var secretKey = _configuration["JwtSettings:SecretKey"] 
            ?? throw new InvalidOperationException("La clave secreta JWT no está configurada.");
        var issuer = _configuration["JwtSettings:Issuer"] ?? "MatoliApi";
        var audience = _configuration["JwtSettings:Audience"] ?? "MatoliFront";
        var expireDaysStr = _configuration["JwtSettings:ExpireDays"] ?? "7";
        if (!double.TryParse(expireDaysStr, out double expireDays))
        {
            expireDays = 7;
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "User"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var expiration = DateTime.UtcNow.AddDays(expireDays);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiration,
            signingCredentials: credentials);

        return new LoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Username = username,
            Expiration = expiration
        };
    }
}
