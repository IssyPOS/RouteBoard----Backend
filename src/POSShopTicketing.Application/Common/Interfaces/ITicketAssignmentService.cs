namespace POSShopTicketing.Application.Common.Interfaces;

/// <summary>
/// Automated routing: "member routes to this Team Member" per
/// AssignmentRule, tried member -> org team -> org -> unassigned
/// (default queue), matching the spec's inbound-pipeline diagram
/// exactly.
/// </summary>
public interface ITicketAssignmentService
{
    Task<Guid?> ResolveAssigneeAsync(
        Guid tenantId,
        Guid? organizationId,
        Guid? organizationTeamId,
        Guid? organizationMemberId,
        CancellationToken cancellationToken);
}
