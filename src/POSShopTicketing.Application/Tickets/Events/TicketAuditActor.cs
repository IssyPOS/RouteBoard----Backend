using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Interfaces;

namespace POSShopTicketing.Application.Tickets.Events;

/// <summary>
/// Shared "who did this" resolution for the ticket audit handlers below.
/// Most ticket events happen inside an authenticated request, where
/// ICurrentUserService reflects the acting Team Member correctly (these
/// notification handlers run synchronously, in the same request scope,
/// as the command that published the event). The one path with no
/// Team Member context at all is the inbound-email webhook - there,
/// "who did this" is either the customer who emailed in (ticket
/// created, or a reply that reopened one) or nobody at all (an
/// assignment rule firing automatically), so the fallback is a
/// parameter rather than one-size-fits-all.
/// </summary>
internal static class TicketAuditActor
{
    public static async Task<(Guid? TeamMemberId, string DisplayName)> ResolveAsync(
        Guid ticketId,
        ICurrentUserService currentUserService,
        IApplicationDbContext context,
        string systemFallbackDisplayName,
        bool useCustomerEmailAsFallback,
        CancellationToken cancellationToken)
    {
        if (currentUserService.TeamMemberId is { } teamMemberId)
        {
            return (teamMemberId, currentUserService.Email ?? teamMemberId.ToString());
        }

        if (!useCustomerEmailAsFallback)
        {
            return (null, systemFallbackDisplayName);
        }

        var rawSenderEmail = await context.Tickets
            .AsNoTracking()
            .Where(t => t.Id == ticketId)
            .Select(t => t.RawSenderEmail)
            .FirstOrDefaultAsync(cancellationToken);

        return (null, string.IsNullOrWhiteSpace(rawSenderEmail) ? systemFallbackDisplayName : $"{rawSenderEmail} (via email)");
    }
}
