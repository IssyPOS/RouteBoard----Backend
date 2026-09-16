using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Domain.Enums;
using POSShopTicketing.Infrastructure.Persistence;

namespace POSShopTicketing.Infrastructure.Services;

/// <summary>
/// "This Organization / OrganizationTeam / OrganizationMember always
/// routes to this Team Member" - tried most-specific first
/// (OrganizationMember), then OrganizationTeam, then Organization,
/// exactly matching "member > org team > org > default queue" from the
/// spec's inbound-pipeline diagram. Ties within the same scope level are
/// broken by AssignmentRule.PriorityOrder. Returns null (the "default
/// queue" - unassigned) if nothing matches.
/// </summary>
public class TicketAssignmentService : ITicketAssignmentService
{
    private readonly ApplicationDbContext _context;

    public TicketAssignmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid?> ResolveAssigneeAsync(
        Guid tenantId,
        Guid? organizationId,
        Guid? organizationDepartmentId,
        Guid? organizationContactId,
        CancellationToken cancellationToken)
    {
        if (organizationContactId.HasValue)
        {
            var byContact = await FindRuleAsync(tenantId, AssignmentScopeType.OrganizationContact, organizationContactId.Value, cancellationToken);
            if (byContact.HasValue)
            {
                return byContact;
            }
        }

        if (organizationDepartmentId.HasValue)
        {
            var byDepartment = await FindRuleAsync(tenantId, AssignmentScopeType.OrganizationDepartment, organizationDepartmentId.Value, cancellationToken);
            if (byDepartment.HasValue)
            {
                return byDepartment;
            }
        }

        if (organizationId.HasValue)
        {
            var byOrg = await FindRuleAsync(tenantId, AssignmentScopeType.Organization, organizationId.Value, cancellationToken);
            if (byOrg.HasValue)
            {
                return byOrg;
            }
        }

        // Default queue: no matching rule, leave unassigned rather than
        // guessing - a Manager can always assign it manually.
        return null;
    }

    private async Task<Guid?> FindRuleAsync(
        Guid tenantId, AssignmentScopeType scopeType, Guid scopeId, CancellationToken cancellationToken)
    {
        return await _context.AssignmentRules
            .AsNoTracking()
            .Where(r => r.TenantId == tenantId && r.IsActive && r.ScopeType == scopeType && r.ScopeId == scopeId)
            .OrderBy(r => r.PriorityOrder)
            .Select(r => (Guid?)r.AssignedToTeamMemberId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
