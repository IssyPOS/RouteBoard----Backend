using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.Common.Interfaces;

/// <summary>Abstracts "who is calling right now" away from HttpContext,
/// so the Application layer never takes a dependency on ASP.NET
/// Core.</summary>
public interface ICurrentUserService
{
    Guid? TeamMemberId { get; }
    string? Email { get; }
    TeamMemberRole? Role { get; }
}
