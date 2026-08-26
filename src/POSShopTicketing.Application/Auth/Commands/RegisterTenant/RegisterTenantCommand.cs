using System.Text.RegularExpressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Auth.Common;
using POSShopTicketing.Application.Auth.Events;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Common.Models;
using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;
using POSShopTicketing.Domain.Exceptions;

namespace POSShopTicketing.Application.Auth.Commands.RegisterTenant;

/// <summary>
/// Self-serve onboarding: "Because other companies will eventually
/// register as Service Providers alongside yours" (spec, section 02).
/// Creates a brand-new Tenant and its first TeamMember as Owner, and
/// logs them straight in - the practical bootstrap path, since nothing
/// else in the system can create the first Owner of a new tenant.
/// </summary>
public record RegisterTenantCommand : IRequest<AuthResultDto>
{
    public string TenantName { get; init; } = string.Empty;
    public string OwnerEmail { get; init; } = string.Empty;
    public string OwnerPassword { get; init; } = string.Empty;
    public string OwnerFirstName { get; init; } = string.Empty;
    public string OwnerLastName { get; init; } = string.Empty;
}

public class RegisterTenantCommandHandler : IRequestHandler<RegisterTenantCommand, AuthResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly AuthResultFactory _authResultFactory;
    private readonly IPublisher _publisher;

    public RegisterTenantCommandHandler(
        IApplicationDbContext context,
        ICurrentTenantService currentTenantService,
        IPasswordHasherService passwordHasher,
        AuthResultFactory authResultFactory,
        IPublisher publisher)
    {
        _context = context;
        _currentTenantService = currentTenantService;
        _passwordHasher = passwordHasher;
        _authResultFactory = authResultFactory;
        _publisher = publisher;
    }

    public async Task<AuthResultDto> Handle(RegisterTenantCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.OwnerEmail.Trim().ToLowerInvariant();

        var emailTaken = await _context.TeamMembers
            .AsNoTracking()
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (emailTaken)
        {
            throw new DomainException($"An account with email \"{normalizedEmail}\" already exists.");
        }

        var slug = await GenerateUniqueSlugAsync(request.TenantName, cancellationToken);

        var tenant = new Tenant
        {
            Name = request.TenantName.Trim(),
            Slug = slug,
            TicketPrefix = BuildTicketPrefix(slug),
            Plan = TenantPlan.Trial,
            Status = TenantStatus.Active
        };

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync(cancellationToken);

        // See ICurrentTenantService.SetTenant: the owner TeamMember row
        // below is RLS-protected, and there's no JWT yet to carry a
        // tenant_id claim for this public bootstrap endpoint - the
        // tenant we just created is the one to scope that write to.
        _currentTenantService.SetTenant(tenant.Id);

        var owner = new TeamMember
        {
            TenantId = tenant.Id,
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.Hash(request.OwnerPassword),
            FirstName = request.OwnerFirstName.Trim(),
            LastName = request.OwnerLastName.Trim(),
            Role = TeamMemberRole.Owner,
            Status = TeamMemberStatus.Active
        };

        _context.TeamMembers.Add(owner);
        await _context.SaveChangesAsync(cancellationToken);

        var result = await _authResultFactory.IssueAsync(owner, cancellationToken);

        await _publisher.Publish(
            new TeamMemberLoggedInEvent(tenant.Id, owner.Id, owner.Email, owner.FullName, owner.Role.ToString()),
            cancellationToken);

        return result;
    }

    private async Task<string> GenerateUniqueSlugAsync(string tenantName, CancellationToken cancellationToken)
    {
        var baseSlug = Regex.Replace(tenantName.Trim().ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
        if (string.IsNullOrEmpty(baseSlug))
        {
            baseSlug = "tenant";
        }

        var slug = baseSlug;
        var suffix = 1;

        while (await _context.Tenants.AsNoTracking().AnyAsync(t => t.Slug == slug, cancellationToken))
        {
            suffix++;
            slug = $"{baseSlug}-{suffix}";
        }

        return slug;
    }

    private static string BuildTicketPrefix(string slug)
    {
        var letters = Regex.Replace(slug, "[^a-zA-Z]", "").ToUpperInvariant();
        return string.IsNullOrEmpty(letters) ? "TKT" : letters[..Math.Min(5, letters.Length)];
    }
}
