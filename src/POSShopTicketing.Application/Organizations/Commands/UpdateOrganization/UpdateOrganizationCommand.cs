using MediatR;
using POSShopTicketing.Application.Organizations.Commands.UpdateOrganization;
using POSShopTicketing.Domain.Enums;

public record UpdateOrganizationCommand : IRequest<UpdateOrganizationDto>
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Domain { get; init; } = string.Empty;

    public OrganizationStatus Status { get; init; }
}