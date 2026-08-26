using POSShopTicketing.Domain.Entities;

namespace POSShopTicketing.Application.OrganizationTeams.Queries.GetOrganizationTeams;

public record OrganizationTeamDto
{
    public Guid Id { get; init; }
    public Guid OrganizationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int MemberCount { get; init; }

    public static OrganizationTeamDto FromEntity(OrganizationTeam entity) => new()
    {
        Id = entity.Id,
        OrganizationId = entity.OrganizationId,
        Name = entity.Name,
        MemberCount = entity.Members.Count
    };
}
