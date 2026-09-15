using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.OrganizationContacts.Queries.GetOrganizationContacts;
using POSShopTicketing.Application.OrganizationMembers.Queries.GetOrganizationContacts;
using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;
using POSShopTicketing.Domain.Exceptions;

namespace POSShopTicketing.Application.Organizations.Commands.UpdateOrganizationContact;

public record UpdateOrganizationContactCommand : IRequest<UpdateOrganizationContactDto>
{
    public Guid ContactOrganizationId { get; init; }
    public Guid OrganizationId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string JobTitle { get; init; } = string.Empty;
    public string? Phone { get; init; }
}

public class UpdateOrganizationContactCommandHandler
    : IRequestHandler<UpdateOrganizationContactCommand, UpdateOrganizationContactDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateOrganizationContactCommandHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateOrganizationContactDto> Handle(
        UpdateOrganizationContactCommand request,
        CancellationToken cancellationToken)
    {

       var entity = await _context.OrganizationContacts.FirstOrDefaultAsync(d => d.Id == request.ContactOrganizationId && d.OrganizationId == request.OrganizationId, cancellationToken) ?? throw new NotFoundException(nameof(UpdateOrganizationContactDto), request.ContactOrganizationId);


        entity.FirstName = request.FirstName.Trim();
        entity.LastName = request.LastName.Trim();
        entity.Email = request.Email.Trim();
        entity.JobTitle = request.JobTitle.Trim();
        entity.Phone = request.Phone.Trim();

        


        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateOrganizationContactDto
        {
            Id = entity.Id,
            OrganizationId = entity.OrganizationId,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Email = entity.Email,
            JobTitle = entity.JobTitle,
            Phone = entity.Phone
        };
    }
}


