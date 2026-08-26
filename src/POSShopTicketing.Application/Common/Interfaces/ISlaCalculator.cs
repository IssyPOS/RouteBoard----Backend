using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.Common.Interfaces;

public interface ISlaCalculator
{
    Task<DateTime?> CalculateDueDateAsync(
        Guid tenantId, TicketPriority priority, DateTime startedAt, CancellationToken cancellationToken);
}
