using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSShopTicketing.Application.TeamMembers.Commands.DisableTeamMember;
using POSShopTicketing.Application.TeamMembers.Commands.UpdateTeamMemberRole;
using POSShopTicketing.Application.TeamMembers.Queries.GetTeamMembers;
using POSShopTicketing.Domain.Enums;
using POSShopTicketing.Shared.Wrappers;

namespace POSShopTicketing.Api.Controllers;

public class TeamMembersController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<TeamMemberDto>>> GetTeamMembers(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await Mediator.Send(new GetTeamMembersQuery { PageNumber = pageNumber, PageSize = pageSize });
        return Ok(PaginatedResponse<TeamMemberDto>.Create(result.Items, result.PageNumber, pageSize, result.TotalCount));
    }

    /// <summary>Owner/Admin only; granting Owner or Admin itself requires
    /// an Owner (enforced in the handler).</summary>
    [Authorize(Roles = "Owner,Admin")]
    [HttpPatch("{id:guid}/role")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateRole(Guid id, [FromBody] TeamMemberRole newRole)
    {
        await Mediator.Send(new UpdateTeamMemberRoleCommand(id, newRole));
        return Ok(ApiResponse<object>.Success(new { }, "Role updated."));
    }

    [Authorize(Roles = "Owner,Admin")]
    [HttpPatch("{id:guid}/disable")]
    public async Task<ActionResult<ApiResponse<object>>> Disable(Guid id)
    {
        await Mediator.Send(new DisableTeamMemberCommand(id));
        return Ok(ApiResponse<object>.Success(new { }, "Team member disabled."));
    }
}
