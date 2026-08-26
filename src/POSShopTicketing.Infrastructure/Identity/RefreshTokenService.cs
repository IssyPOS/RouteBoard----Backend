using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using POSShopTicketing.Application.Common.Interfaces;

namespace POSShopTicketing.Infrastructure.Identity;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly JwtSettings _settings;

    public RefreshTokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public (string PlainTextToken, string TokenHash, DateTime ExpiresAt) GenerateToken()
    {
        var plainTextToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');

        var expiresAt = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpiryDays);

        return (plainTextToken, Hash(plainTextToken), expiresAt);
    }

    public string Hash(string plainTextToken)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(plainTextToken));
        return Convert.ToHexString(bytes);
    }
}
