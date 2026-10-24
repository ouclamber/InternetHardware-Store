using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Backend.Domain.Shared;

namespace Backend.Infrastructure.Services;

public class JwtService : IJwtService
{
    public const int MinSecretKeyLength = 32;
    public const int DefaultExpiresMinutes = 60 * 24; // 1 день

    /// <summary>Имя claim с меткой безопасности пользователя.</summary>
    public const string SecurityStampClaim = "sstamp";

    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiresMinutes;

    public JwtService(IConfiguration configuration)
    {
        // Секрет обязателен и не имеет значения по умолчанию.
        var secret = configuration["Jwt:SecretKey"];
        if (string.IsNullOrWhiteSpace(secret) || secret.Length < MinSecretKeyLength)
            throw new InvalidOperationException(
                $"Jwt:SecretKey не задан или короче {MinSecretKeyLength} символов");

        _secretKey = secret;
        _issuer = configuration["Jwt:Issuer"] ?? "InternetHardwareStore";
        _audience = configuration["Jwt:Audience"] ?? "InternetHardwareStoreClient";

        _expiresMinutes = int.TryParse(configuration["Jwt:ExpiresMinutes"], out var minutes) && minutes > 0
            ? minutes
            : DefaultExpiresMinutes;
    }

    public string GenerateToken(int userId, string userName, string role, string securityStamp)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, userName),
            new Claim(ClaimTypes.Role, role),
            new Claim("userId", userId.ToString()),
            new Claim("userName", userName),
            new Claim("role", role),
            new Claim(SecurityStampClaim, securityStamp ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_expiresMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
