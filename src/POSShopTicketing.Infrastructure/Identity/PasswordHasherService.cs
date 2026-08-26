using Microsoft.AspNetCore.Identity;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Domain.Entities;

namespace POSShopTicketing.Infrastructure.Identity;

/// <summary>Thin wrapper around ASP.NET Core Identity's PBKDF2 hasher,
/// used standalone purely for hashing/verifying - no full Identity
/// membership system.</summary>
public class PasswordHasherService : IPasswordHasherService
{
    private readonly PasswordHasher<TeamMember> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(default!, password);

    public bool Verify(string hash, string providedPassword)
    {
        if (string.IsNullOrEmpty(hash))
        {
            return false;
        }

        var result = _hasher.VerifyHashedPassword(default!, hash, providedPassword);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
