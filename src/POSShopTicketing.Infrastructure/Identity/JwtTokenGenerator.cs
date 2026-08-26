using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Domain.Entities;

namespace POSShopTicketing.Infrastructure.Identity;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _settings;

    public JwtTokenGenerator(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public (string Token, DateTime ExpiresAt) GenerateAccessToken(TeamMember teamMember)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, teamMember.Id.ToString()),
            new(ClaimTypes.Email, teamMember.Email),
            new(ClaimTypes.Name, teamMember.FullName),
            new(ClaimTypes.Role, teamMember.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Omitted entirely for PlatformSuperAdmin (TenantId sentinel is
        // Guid.Empty) - ICurrentTenantService.TenantId then reads back as
        // null, which is exactly what both the EF global query filters
        // and the RLS policies expect for "no tenant".
        if (teamMember.TenantId != Guid.Empty)
        {
            claims.Add(new Claim("tenant_id", teamMember.TenantId.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
