using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.OrganizationMembers.Queries.GetOrganizationMembers;

public record OrganizationMemberDto
{
    public Guid Id { get; init; }
    public Guid OrganizationId { get; init; }
    public string? OrganizationName { get; init; }
    public Guid? OrganizationTeamId { get; init; }
    public string? OrganizationTeamName { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public OrganizationMemberStatus Status { get; init; }

    public static OrganizationMemberDto FromEntity(OrganizationMember entity) => new()
    {
        Id = entity.Id,
        OrganizationId = entity.OrganizationId,
        OrganizationName = entity.Organization?.Name,
        OrganizationTeamId = entity.OrganizationTeamId,
        OrganizationTeamName = entity.OrganizationTeam?.Name,
        FullName = entity.FullName,
        Email = entity.Email,
        Phone = entity.Phone,
        Status = entity.Status
    };
}
