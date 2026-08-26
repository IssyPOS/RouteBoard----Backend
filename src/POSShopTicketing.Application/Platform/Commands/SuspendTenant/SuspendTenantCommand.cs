using MediatR;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;
using POSShopTicketing.Domain.Exceptions;

namespace POSShopTicketing.Application.Platform.Commands.SuspendTenant;

public record SuspendTenantCommand(Guid TenantId) : IRequest;

public class SuspendTenantCommandHandler : IRequestHandler<SuspendTenantCommand>
{
    private readonly IApplicationDbContext _context;

    public SuspendTenantCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(SuspendTenantCommand request, CancellationToken cancellationToken)
    {
        if (request.TenantId == Guid.Empty)
        {
            throw new DomainException("The reserved Platform tenant cannot be suspended.");
        }

        var tenant = await _context.Tenants.FindAsync(new object[] { request.TenantId }, cancellationToken)
            ?? throw new NotFoundException(nameof(Tenant), request.TenantId);

        tenant.Status = TenantStatus.Suspended;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
