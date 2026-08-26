using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.Common.Services;

/// <summary>
/// SLA management (Phase 5, already wired up): looks up the Tenant's
/// SlaPolicy for a Priority and returns the resolution due date. Returns
/// null if the tenant hasn't configured SLA policies yet - SLA tracking
/// is opt-in per tenant, not a forced default.
/// </summary>
public class SlaCalculator : ISlaCalculator
{
    private readonly IApplicationDbContext _context;

    public SlaCalculator(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DateTime?> CalculateDueDateAsync(
        Guid tenantId, TicketPriority priority, DateTime startedAt, CancellationToken cancellationToken)
    {
        var policy = await _context.SlaPolicies
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Priority == priority, cancellationToken);

        return policy is null ? null : startedAt.AddMinutes(policy.ResolutionTargetMinutes);
    }
}
