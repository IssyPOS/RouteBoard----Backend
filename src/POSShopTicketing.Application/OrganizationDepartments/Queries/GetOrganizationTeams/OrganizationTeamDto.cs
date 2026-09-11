using POSShopTicketing.Domain.Entities;

namespace POSShopTicketing.Application.OrganizationTeams.Queries.GetOrganizationTeams;

public record OrganizationDepartmentDto
{
    public Guid Id { get; init; }
    public Guid OrganizationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int ContactCount { get; init; }
    public DateTime CreatedAt { get; init; }

    public static OrganizationDepartmentDto FromEntity(OrganizationDepartment entity) => new()
    {
        Id = entity.Id,
        OrganizationId = entity.OrganizationId,
        Name = entity.Name,
        ContactCount = entity.Contacts.Count,
        CreatedAt = entity.CreatedAt
    };
}
