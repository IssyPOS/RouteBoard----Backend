using MediatR;

namespace POSShopTicketing.Application.Auth.Events;

public record TeamMemberLoggedOutEvent(
    Guid? TenantId, Guid TeamMemberId, string Email, string FullName, string Role) : INotification;
