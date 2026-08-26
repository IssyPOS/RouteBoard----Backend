using MediatR;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;
using POSShopTicketing.Domain.Exceptions;

namespace POSShopTicketing.Application.Platform.Commands.ReactivateTenant;

public record ReactivateTenantCommand(Guid TenantId) : IRequest;

public class ReactivateTenantCommandHandler : IRequestHandler<ReactivateTenantCommand>
{
    private readonly IApplicationDbContext _context;

    public ReactivateTenantCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ReactivateTenantCommand request, CancellationToken cancellationToken)
    {
        if (request.TenantId == Guid.Empty)
        {
            throw new DomainException("The reserved Platform tenant does not need reactivating.");
        }

        var tenant = await _context.Tenants.FindAsync(new object[] { request.TenantId }, cancellationToken)
            ?? throw new NotFoundException(nameof(Tenant), request.TenantId);

        tenant.Status = TenantStatus.Active;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
