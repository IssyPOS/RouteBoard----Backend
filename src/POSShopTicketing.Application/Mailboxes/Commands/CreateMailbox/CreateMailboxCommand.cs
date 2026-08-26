using System.Security.Cryptography;
using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;
using POSShopTicketing.Domain.Exceptions;

namespace POSShopTicketing.Application.Mailboxes.Commands.CreateMailbox;

/// <summary>
/// Two ways to get a working inbound address, per the spec's "The
/// address a client emails is one you control" section:
///
///   - Omit EmailAddress -> zero-setup system-provided address
///     ({tenant-slug}@{platform's configured inbound domain}), verified
///     immediately - no DNS action needed from this tenant at all.
///   - Supply EmailAddress -> the tenant's own domain, forwarded to this
///     app's webhook - requires a real DNS/forwarding change on their
///     end and a follow-up POST .../verify call once that's live.
/// </summary>
public record CreateMailboxCommand : IRequest<Guid>
{
    public string? EmailAddress { get; init; }
    public MailboxProvider Provider { get; init; } = MailboxProvider.SendGridInboundParse;
    public bool IsDefault { get; init; }
}

public class CreateMailboxCommandHandler : IRequestHandler<CreateMailboxCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly ISystemMailboxAddressProvider _systemMailboxAddressProvider;

    public CreateMailboxCommandHandler(
        IApplicationDbContext context,
        ICurrentTenantService currentTenantService,
        ISystemMailboxAddressProvider systemMailboxAddressProvider)
    {
        _context = context;
        _currentTenantService = currentTenantService;
        _systemMailboxAddressProvider = systemMailboxAddressProvider;
    }

    public async Task<Guid> Handle(CreateMailboxCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenantService.TenantId
            ?? throw new ForbiddenException("Mailboxes must be created from within a tenant.");

        var isSystemProvided = string.IsNullOrWhiteSpace(request.EmailAddress);

        string normalizedAddress;
        if (isSystemProvided)
        {
            var tenant = await _context.Tenants.FindAsync(new object[] { tenantId }, cancellationToken)
                ?? throw new NotFoundException(nameof(Tenant), tenantId);

            normalizedAddress = _systemMailboxAddressProvider.BuildAddress(tenant.Slug);
        }
        else
        {
            normalizedAddress = request.EmailAddress!.Trim().ToLowerInvariant();

            if (_systemMailboxAddressProvider.IsSystemDomain(normalizedAddress))
            {
                throw new DomainException(
                    "That address is on the platform's own system-provided domain - omit EmailAddress " +
                    "entirely to get a zero-setup system-provided mailbox instead of supplying one here.");
            }
        }

        var duplicate = await _context.Mailboxes
            .AsNoTracking()
            .AnyAsync(m => m.EmailAddress == normalizedAddress, cancellationToken);

        if (duplicate)
        {
            throw new DomainException($"Mailbox \"{normalizedAddress}\" is already registered.");
        }

        if (request.IsDefault)
        {
            var existingDefaults = await _context.Mailboxes
                .Where(m => m.TenantId == tenantId && m.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var mailbox in existingDefaults)
            {
                mailbox.IsDefault = false;
            }
        }

        var entity = new Mailbox
        {
            TenantId = tenantId,
            EmailAddress = normalizedAddress,
            Provider = request.Provider,
            WebhookSecret = GenerateWebhookSecret(),
            // A system-provided address rides on the domain the platform
            // operator already verified once - nothing left for this
            // tenant to confirm, so it's live immediately.
            IsVerified = isSystemProvided,
            IsSystemProvided = isSystemProvided,
            IsDefault = request.IsDefault
        };

        _context.Mailboxes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    private static string GenerateWebhookSecret() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
}
