using MediatR;

namespace POSShopTicketing.Application.Auth.Events;

/// <summary>Raised on Login, tenant self-registration (the new Owner's
/// first session), and invite acceptance (the new member's first
/// session) - deliberately NOT on every token refresh, which just
/// extends an existing session rather than starting a new one.</summary>
public record TeamMemberLoggedInEvent(
    Guid? TenantId, Guid TeamMemberId, string Email, string FullName, string Role) : INotification;
