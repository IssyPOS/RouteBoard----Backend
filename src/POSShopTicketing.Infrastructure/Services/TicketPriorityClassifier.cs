using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Infrastructure.Services;

/// <summary>
/// Simple keyword rule used only when the caller doesn't set a Priority
/// explicitly. Phase 4 in the roadmap calls for real "keyword/org-based
/// auto-priority rules" once there's enough ticket volume to tune them -
/// this sits behind ITicketPriorityClassifier specifically so it can be
/// swapped for that later without touching any command handler.
/// </summary>
public class TicketPriorityClassifier : ITicketPriorityClassifier
{
    private static readonly string[] UrgentKeywords =
    {
        "down", "outage", "can't log in", "cannot log in", "data loss", "security breach", "emergency"
    };

    private static readonly string[] HighKeywords =
    {
        "urgent", "asap", "broken", "not working", "critical", "blocked", "failing"
    };

    public TicketPriority Classify(string subject, string? description)
    {
        var text = $"{subject} {description}".ToLowerInvariant();

        if (UrgentKeywords.Any(text.Contains))
        {
            return TicketPriority.Urgent;
        }

        if (HighKeywords.Any(text.Contains))
        {
            return TicketPriority.High;
        }

        return TicketPriority.Medium;
    }
}
