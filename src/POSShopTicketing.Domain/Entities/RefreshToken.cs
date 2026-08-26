using POSShopTicketing.Domain.Common;

namespace POSShopTicketing.Domain.Entities;

/// <summary>Rotating refresh token behind POST /auth/refresh. Storing
/// only a hash means a leaked database dump can't be replayed as
/// tokens.</summary>
public class RefreshTokenz : BaseEntity
{
    public Guid TeamMemberId { get; set; }
    public TeamMember? TeamMember { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    /// <summary>Set when this token was rotated out for a newer one -
    /// lets us detect and react to reuse of a stolen, already-rotated
    /// token.</summary>
    public string? ReplacedByTokenHash { get; set; }

    public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;
}
