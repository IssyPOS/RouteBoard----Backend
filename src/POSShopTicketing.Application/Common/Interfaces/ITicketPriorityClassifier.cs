using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.Common.Interfaces;

public interface ITicketPriorityClassifier
{
    TicketPriority Classify(string subject, string? description);
}
