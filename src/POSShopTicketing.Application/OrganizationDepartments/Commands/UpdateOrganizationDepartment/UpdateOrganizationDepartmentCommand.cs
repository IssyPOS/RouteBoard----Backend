using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.OrganizationDepartments.Queries.GetOrganizationDepartments;
using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;
using POSShopTicketing.Domain.Exceptions;

namespace POSShopTicketing.Application.Organizations.Commands.UpdateOrganizationDepartment;

public record UpdateOrganizationDepartmentCommand : IRequest<UpdateOrganizationDepartmentDto>
{
    public Guid OrganizationId { get; init; }

    public Guid DepartmentId { get; init; }

    public string Name { get; init; } = string.Empty;
}

public class UpdateOrganizationDepartmentCommandHandler  : IRequestHandler<UpdateOrganizationDepartmentCommand, UpdateOrganizationDepartmentDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateOrganizationDepartmentCommandHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateOrganizationDepartmentDto> Handle(UpdateOrganizationDepartmentCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.OrganizationDepartments
            .FirstOrDefaultAsync(
                d => d.Id == request.DepartmentId &&
                     d.OrganizationId == request.OrganizationId,
                cancellationToken)
            ?? throw new NotFoundException(
                nameof(OrganizationDepartment),
                request.DepartmentId);

        entity.Name = request.Name.Trim();

        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateOrganizationDepartmentDto
        {
            Id = entity.Id,
            Name = entity.Name,
            OrganizationId = entity.OrganizationId
        };
    }

}

