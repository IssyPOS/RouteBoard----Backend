using POSShopTicketing.Domain.Entities;

namespace POSShopTicketing.Application.Common.Interfaces;

/// <summary>Issues and rotates the long-lived refresh token behind
/// POST /auth/refresh. Only the hash is ever persisted (see
/// RefreshToken.TokenHash) - the plaintext token is returned to the
/// caller exactly once, at issuance.</summary>
public interface IRefreshTokenService
{
    (string PlainTextToken, string TokenHash, DateTime ExpiresAt) GenerateToken();
    string Hash(string plainTextToken);
}
