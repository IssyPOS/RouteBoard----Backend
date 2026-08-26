using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Auth.Events;
using POSShopTicketing.Application.Common.Interfaces;

namespace POSShopTicketing.Application.Auth.Commands.Logout;

/// <summary>
/// POST /auth/logout - revokes the presented refresh token so it can
/// never be redeemed again (the access token itself, being a stateless
/// JWT, simply expires on its own short timer - there's nothing to
/// revoke there). Deliberately tolerant of an already-invalid token
/// (expired, already revoked, or simply unrecognized) rather than
/// throwing UnauthorizedException: the caller's goal - "make sure I'm
/// logged out" - is already satisfied in every one of those cases, and
/// a client that's a little out of sync with the server (e.g. retrying
/// after a network hiccup) shouldn't see an error for it.
/// </summary>
public record LogoutCommand(string RefreshToken) : IRequest;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IDateTime _dateTime;
    private readonly IPublisher _publisher;

    public LogoutCommandHandler(
        IApplicationDbContext context,
        IRefreshTokenService refreshTokenService,
        IDateTime dateTime,
        IPublisher publisher)
    {
        _context = context;
        _refreshTokenService = refreshTokenService;
        _dateTime = dateTime;
        _publisher = publisher;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var presentedHash = _refreshTokenService.Hash(request.RefreshToken);

        var token = await _context.RefreshTokens
            .IgnoreQueryFilters()
            .Include(t => t.TeamMember)
            .FirstOrDefaultAsync(t => t.TokenHash == presentedHash, cancellationToken);

        if (token is null)
        {
            return;
        }

        if (token.RevokedAt is null)
        {
            token.RevokedAt = _dateTime.Now;
            await _context.SaveChangesAsync(cancellationToken);
        }

        if (token.TeamMember is { } teamMember)
        {
            await _publisher.Publish(
                new TeamMemberLoggedOutEvent(
                    teamMember.TenantId == Guid.Empty ? null : teamMember.TenantId,
                    teamMember.Id, teamMember.Email, teamMember.FullName, teamMember.Role.ToString()),
                cancellationToken);
        }
    }
}
