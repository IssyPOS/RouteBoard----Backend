using MediatR;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.Organizations.Commands.UpdateOrganization;

public record UpdateOrganizationCommand : IRequest
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Domain { get; init; } = string.Empty;
    public OrganizationStatus Status { get; init; }
}

public class UpdateOrganizationCommandHandler : IRequestHandler<UpdateOrganizationCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateOrganizationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateOrganizationCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Organizations.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new NotFoundException(nameof(Organization), request.Id);

        entity.Name = request.Name.Trim();
        entity.Domain = request.Domain.Trim();
        entity.Status = request.Status;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
