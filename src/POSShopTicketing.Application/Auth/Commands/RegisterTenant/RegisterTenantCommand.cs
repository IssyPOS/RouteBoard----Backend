using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Auth.Common;
using POSShopTicketing.Application.Auth.Events;
using POSShopTicketing.Application.Auth.Queries.RegisterTenantDto;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Common.Models;
using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;
using POSShopTicketing.Domain.Exceptions;
using System.Net;
using System.Text.RegularExpressions;

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
    public string Slug { get; init; } = string.Empty;
    public List<SignupInviteDto> Invites { get; init; } = new();
}

public class RegisterTenantCommandHandler : IRequestHandler<RegisterTenantCommand, AuthResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IEmailSender _emailSender;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly AuthResultFactory _authResultFactory;
    private readonly IPublisher _publisher;
    private readonly IDateTime _dateTime;
    private readonly IAppUrlProvider _appUrlProvider;

    private readonly IRefreshTokenService _tokenService;

    public RegisterTenantCommandHandler(
        IApplicationDbContext context,
        ICurrentTenantService currentTenantService,
        ICurrentUserService currentUserService,
        IPasswordHasherService passwordHasher,
        AuthResultFactory authResultFactory,
        IPublisher publisher,
        IRefreshTokenService tokenService,
        IEmailSender emailSender,
        IAppUrlProvider appUrlProvider,
        IDateTime dateTime)
    {
        _context = context;
        _currentTenantService = currentTenantService;
        _currentUserService = currentUserService;
        _passwordHasher = passwordHasher;
        _authResultFactory = authResultFactory;
        _publisher = publisher;
        _tokenService = tokenService;
        _appUrlProvider = appUrlProvider;
        _emailSender = emailSender;
        _dateTime = dateTime;
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

        //var slug = await GenerateUniqueSlugAsync(request.TenantName, cancellationToken);

        var tenant = new Tenant
        {
            Name = request.TenantName.Trim(),
            Slug = request.Slug,
            TicketPrefix = BuildTicketPrefix(request.Slug),
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

      
        var expiresAt = _dateTime.Now.AddDays(7);

        foreach (var invite in request.Invites)
        {
            if (string.IsNullOrWhiteSpace(invite.Email) || !invite.Role.HasValue)
            {
                continue;
            }

            var (plainTextToken, tokenHash, _) = _tokenService.GenerateToken();

            var teamMemberInvite = new TeamMember
            {
                TenantId = tenant.Id,
                Email = invite.Email.Trim().ToLowerInvariant(),
                Role = invite.Role.Value,
                Status = TeamMemberStatus.Invited,
                InviteTokenHash = tokenHash,
                InviteTokenExpiresAt = expiresAt
            };

            _context.TeamMembers.Add(teamMemberInvite);

            await SendInviteEmailAsync(
                teamMemberInvite,
                tenant.Name,
                plainTextToken,
                expiresAt,
                cancellationToken);
        }


        await _context.SaveChangesAsync(cancellationToken);

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

       // await SendInviteEmailAsync(owner, tenant.Name, plainTextToken, expiresAt, cancellationToken);

        return result;
    }

    private async Task SendInviteEmailAsync(
        TeamMember teamMember, string tenantName, string plainTextToken, DateTime expiresAt, CancellationToken cancellationToken)
    {
        var acceptUrl = _appUrlProvider.BuildInviteAcceptUrl(plainTextToken);
        var inviterLabel = _currentUserService.Email ?? "a team admin";
        var encodedTenantName = WebUtility.HtmlEncode(tenantName);
        var encodedToken = WebUtility.HtmlEncode(plainTextToken);

        var bodyHtml = string.IsNullOrEmpty(acceptUrl)
            ? $"<p>{WebUtility.HtmlEncode(inviterLabel)} invited you to join <strong>{encodedTenantName}</strong> " +
              $"on POSShopTicketing as {teamMember.Role}.</p>" +
              $"<p>To accept, call <code>POST /api/auth/invite/accept</code> with this token:</p>" +
              $"<p style=\"font-family:monospace;background:#f4f4f4;padding:8px;word-break:break-all\">{encodedToken}</p>" +
              $"<p>This invite expires {expiresAt:yyyy-MM-dd HH:mm} UTC.</p>"
            : $"<p>{WebUtility.HtmlEncode(inviterLabel)} invited you to join <strong>{encodedTenantName}</strong> " +
              $"on POSShopTicketing as {teamMember.Role}.</p>" +
              $"<p><a href=\"{acceptUrl}\">Click here to accept your invitation</a></p>" +
              $"<p>Or use this token directly with <code>POST /api/auth/invite/accept</code>: " +
              $"<span style=\"font-family:monospace\">{encodedToken}</span></p>" +
              $"<p>This invite expires {expiresAt:yyyy-MM-dd HH:mm} UTC.</p>";

        await _emailSender.SendAsync(
            teamMember.Email, teamMember.FullName,
            $"You're invited to join {tenantName} on POSShopTicketing", bodyHtml, cancellationToken);
    }

    //private async Task<string> GenerateUniqueSlugAsync(string tenantName, CancellationToken cancellationToken)
    //{
    //    var baseSlug = Regex.Replace(tenantName.Trim().ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
    //    if (string.IsNullOrEmpty(baseSlug))
    //    {
    //        baseSlug = "tenant";
    //    }

    //    var slug = baseSlug;
    //    var suffix = 1;

    //    while (await _context.Tenants.AsNoTracking().AnyAsync(t => t.Slug == slug, cancellationToken))
    //    {
    //        suffix++;
    //        slug = $"{baseSlug}-{suffix}";
    //    }

    //    return slug;
    //}

    private static string BuildTicketPrefix(string slug)
    {
        var letters = Regex.Replace(slug, "[^a-zA-Z]", "").ToUpperInvariant();
        return string.IsNullOrEmpty(letters) ? "TKT" : letters[..Math.Min(5, letters.Length)];
    }
}
