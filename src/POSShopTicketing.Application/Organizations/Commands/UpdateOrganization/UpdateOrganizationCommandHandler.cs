using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Domain.Entities;

namespace POSShopTicketing.Application.Organizations.Commands.UpdateOrganization;

public class UpdateOrganizationCommandHandler
    : IRequestHandler<UpdateOrganizationCommand, UpdateOrganizationDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateOrganizationCommandHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateOrganizationDto> Handle(
        UpdateOrganizationCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.Organizations
            .FirstOrDefaultAsync(
                o => o.Id == request.Id,
                cancellationToken)
            ?? throw new NotFoundException(
                nameof(Organization),
                request.Id);

        entity.Name = request.Name.Trim();
        entity.Domain = request.Domain.Trim();
        entity.Status = request.Status;

        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateOrganizationDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Domain = entity.Domain,
            Status = entity.Status
        };
    }
}