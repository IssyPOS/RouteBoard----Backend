using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSShopTicketing.Application.OrganizationMembers.Commands.CreateOrganizationMember;
using POSShopTicketing.Application.OrganizationMembers.Commands.MergeOrganizationMember;
using POSShopTicketing.Application.OrganizationMembers.Queries.GetOrganizationMembers;
using POSShopTicketing.Application.Organizations.Commands.CreateOrganization;
using POSShopTicketing.Application.Organizations.Commands.UpdateOrganization;
using POSShopTicketing.Application.Organizations.Queries.GetOrganizationById;
using POSShopTicketing.Application.Organizations.Queries.GetOrganizations;
using POSShopTicketing.Application.OrganizationTeams.Commands.CreateOrganizationTeam;
using POSShopTicketing.Application.OrganizationTeams.Queries.GetOrganizationTeams;
using POSShopTicketing.Shared.Wrappers;

namespace POSShopTicketing.Api.Controllers;

/// <summary>Client Organizations, and nested under them, Organization
/// Teams and Organization Members. "Admin: Manage ... orgs, org teams" -
/// writes are Owner/Admin only; reads are open to any tenant role.</summary>
public class OrganizationsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<OrganizationDto>>> GetOrganizations(
        [FromQuery] string? searchTerm, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await Mediator.Send(new GetOrganizationsQuery { SearchTerm = searchTerm, PageNumber = pageNumber, PageSize = pageSize });
        return Ok(PaginatedResponse<OrganizationDto>.Create(result.Items, result.PageNumber, pageSize, result.TotalCount));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<OrganizationDto>>> GetOrganization(Guid id)
    {
        var result = await Mediator.Send(new GetOrganizationByIdQuery(id));
        return Ok(ApiResponse<OrganizationDto>.Success(result));
    }

    [Authorize(Roles = "Owner,Admin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateOrganization(CreateOrganizationCommand command)
    {
        var id = await Mediator.Send(command);
        var response = ApiResponse<Guid>.Success(id, "Organization created.");
        return CreatedAtAction(nameof(GetOrganization), new { id }, response);
    }

    [Authorize(Roles = "Owner,Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateOrganization(Guid id, UpdateOrganizationCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(ApiResponse<object>.Failure("Route id and body id must match."));
        }

        await Mediator.Send(command);
        return Ok(ApiResponse<object>.Success(new { }, "Organization updated."));
    }

    // ---- Organization Teams -------------------------------------------

    [HttpGet("{id:guid}/teams")]
    public async Task<ActionResult<ApiResponse<List<OrganizationTeamDto>>>> GetTeams(Guid id)
    {
        var result = await Mediator.Send(new GetOrganizationTeamsQuery(id));
        return Ok(ApiResponse<List<OrganizationTeamDto>>.Success(result));
    }

    [Authorize(Roles = "Owner,Admin")]
    [HttpPost("{id:guid}/teams")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateTeam(Guid id, [FromBody] CreateTeamRequest request)
    {
        var teamId = await Mediator.Send(new CreateOrganizationTeamCommand { OrganizationId = id, Name = request.Name });
        return Ok(ApiResponse<Guid>.Success(teamId, "Team created."));
    }

    // ---- Organization Members ------------------------------------------

    [HttpGet("{id:guid}/members")]
    public async Task<ActionResult<PaginatedResponse<OrganizationMemberDto>>> GetMembers(
        Guid id, [FromQuery] string? searchTerm, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await Mediator.Send(new GetOrganizationMembersQuery
        {
            OrganizationId = id, SearchTerm = searchTerm, PageNumber = pageNumber, PageSize = pageSize
        });
        return Ok(PaginatedResponse<OrganizationMemberDto>.Create(result.Items, result.PageNumber, pageSize, result.TotalCount));
    }

    [Authorize(Roles = "Owner,Admin,Manager")]
    [HttpPost("{id:guid}/members")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateMember(Guid id, [FromBody] CreateMemberRequest request)
    {
        var memberId = await Mediator.Send(new CreateOrganizationMemberCommand
        {
            OrganizationId = id,
            OrganizationTeamId = request.OrganizationTeamId,
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone
        });
        return Ok(ApiResponse<Guid>.Success(memberId, "Member created."));
    }

    /// <summary>Dedupe an alternate/typo'd address for someone already known.</summary>
    [Authorize(Roles = "Owner,Admin,Manager")]
    [HttpPost("{id:guid}/members/{memberId:guid}/merge")]
    public async Task<ActionResult<ApiResponse<object>>> MergeMember(Guid id, Guid memberId, [FromBody] Guid mergeIntoMemberId)
    {
        await Mediator.Send(new MergeOrganizationMemberCommand(id, memberId, mergeIntoMemberId));
        return Ok(ApiResponse<object>.Success(new { }, "Members merged."));
    }
}

public record CreateTeamRequest(string Name);
public record CreateMemberRequest(Guid? OrganizationTeamId, string FullName, string Email, string? Phone);
