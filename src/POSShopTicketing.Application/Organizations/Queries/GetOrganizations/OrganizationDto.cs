using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.Organizations.Queries.GetOrganizations;

public record OrganizationDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public OrganizationStatus Status { get; init; }
    public int TeamCount { get; init; }
    public int MemberCount { get; init; }
    public DateTime CreatedAt { get; init; }

    public static OrganizationDto FromEntity(Organization entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Status = entity.Status,
        TeamCount = entity.Teams.Count,
        MemberCount = entity.Members.Count,
        CreatedAt = entity.CreatedAt
    };
}
