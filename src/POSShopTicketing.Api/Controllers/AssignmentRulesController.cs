using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSShopTicketing.Application.AssignmentRules.Commands.CreateAssignmentRule;
using POSShopTicketing.Application.AssignmentRules.Queries.GetAssignmentRules;
using POSShopTicketing.Shared.Wrappers;

namespace POSShopTicketing.Api.Controllers;

/// <summary>"This Organization / Organization Team / Organization Member
/// always routes to this Team Member." Writes are Owner/Admin only
/// ("Admin: Manage ... routing rules").</summary>
public class AssignmentRulesController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AssignmentRuleDto>>>> GetAssignmentRules()
    {
        var result = await Mediator.Send(new GetAssignmentRulesQuery());
        return Ok(ApiResponse<List<AssignmentRuleDto>>.Success(result));
    }

    [Authorize(Roles = "Owner,Admin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateAssignmentRule(CreateAssignmentRuleCommand command)
    {
        var id = await Mediator.Send(command);
        return Ok(ApiResponse<Guid>.Success(id, "Assignment rule created."));
    }
}
