namespace POSShopTicketing.Domain.Enums;

/// <summary>Lookup cascade order for CreateTicket's assignment logic:
/// Member is tried first (most specific), then OrganizationTeam, then
/// Organization (least specific) - matching "member > org team > org >
/// default queue" from the spec's inbound pipeline diagram.</summary>
public enum AssignmentScopeType
{
    OrganizationMember = 0,
    OrganizationTeam = 1,
    Organization = 2
}
