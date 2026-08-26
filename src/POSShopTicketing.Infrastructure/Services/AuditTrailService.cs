using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Common.Models;
using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Infrastructure.Services;

/// <summary>
/// Persists every AuditTrailEntry to the AuditLog table (always), and -
/// if AuditEmail:Enabled and the entry has a TenantId - emails a plain
/// one-line summary to every active Owner/Admin on that tenant via
/// IEmailSender. Which platform actually receives that email depends
/// entirely on how IEmailSender is configured (see SmtpEmailSender) -
/// this service has no idea whether that's SendGrid, Postmark, Amazon
/// SES, Hostinger's own mail hosting, or anything else.
/// </summary>
public class AuditTrailService : IAuditTrailService
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly IDateTime _dateTime;
    private readonly AuditEmailSettings _settings;
    private readonly ILogger<AuditTrailService> _logger;

    public AuditTrailService(
        IApplicationDbContext context,
        IEmailSender emailSender,
        IDateTime dateTime,
        IOptions<AuditEmailSettings> settings,
        ILogger<AuditTrailService> logger)
    {
        _context = context;
        _emailSender = emailSender;
        _dateTime = dateTime;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task RecordAsync(AuditTrailEntry entry, CancellationToken cancellationToken)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            TenantId = entry.TenantId,
            ActorTeamMemberId = entry.ActorTeamMemberId,
            Action = entry.Action,
            EntityType = entry.EntityType,
            EntityId = entry.EntityId,
            Details = entry.Summary,
            CreatedAt = _dateTime.Now
        });

        await _context.SaveChangesAsync(cancellationToken);

        if (!_settings.Enabled || entry.TenantId is not { } tenantId)
        {
            return;
        }

        var recipients = await _context.TeamMembers
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(u => u.TenantId == tenantId
                        && u.Status == TeamMemberStatus.Active
                        && (u.Role == TeamMemberRole.Owner || u.Role == TeamMemberRole.Admin))
            .Select(u => new { u.Email, FullName = u.FirstName + " " + u.LastName })
            .ToListAsync(cancellationToken);

        if (recipients.Count == 0)
        {
            _logger.LogWarning("Audit event {Action} on tenant {TenantId} has no active Owner/Admin to email", entry.Action, tenantId);
            return;
        }

        var subject = $"[POSShopTicketing] {entry.Action}: {entry.ActorDisplayName}";
        var bodyHtml = $"<p>{System.Net.WebUtility.HtmlEncode(entry.Summary)}</p>" +
                       $"<p style=\"color:#666;font-size:12px\">{_dateTime.Now:yyyy-MM-dd HH:mm:ss} UTC</p>";

        foreach (var recipient in recipients)
        {
            await _emailSender.SendAsync(recipient.Email, recipient.FullName, subject, bodyHtml, cancellationToken);
        }
    }
}
