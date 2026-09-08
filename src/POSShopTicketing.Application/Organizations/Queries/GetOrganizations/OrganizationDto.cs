using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.Organizations.Queries.GetOrganizations;

public record OrganizationDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;

    public string Domain { get; init; } = string.Empty;
    public OrganizationStatus Status { get; init; }
    public int DepartmentCount { get; init; }
    public int ContactCount { get; init; }
    public DateTime CreatedAt { get; init; }

    public static OrganizationDto FromEntity(Organization entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Domain = entity.Domain,
        Status = entity.Status,
        DepartmentCount = entity.Departments.Count,
        ContactCount = entity.Contacts.Count,
        CreatedAt = entity.CreatedAt
    };
}
