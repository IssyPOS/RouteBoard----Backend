using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSShopTicketing.Application.Platform.Commands.ReactivateTenant;
using POSShopTicketing.Application.Platform.Commands.SuspendTenant;
using POSShopTicketing.Application.Platform.Queries.GetTenantById;
using POSShopTicketing.Application.Platform.Queries.GetTenants;
using POSShopTicketing.Shared.Wrappers;

namespace POSShopTicketing.Api.Controllers;

/// <summary>
/// PlatformSuperAdmin only - "Manage tenants & billing", "notably
/// cannot: See tenant ticket content day-to-day". Every action here is
/// cross-tenant by design, which is exactly why it's fenced off to this
/// one role and kept in its own controller rather than folded into
/// TicketsController/OrganizationsController/etc.
/// </summary>
[Authorize(Roles = "PlatformSuperAdmin")]
[Route("api/platform/tenants")]
public class PlatformController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<TenantDto>>> GetTenants(
        [FromQuery] string? searchTerm, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await Mediator.Send(new GetTenantsQuery { SearchTerm = searchTerm, PageNumber = pageNumber, PageSize = pageSize });
        return Ok(PaginatedResponse<TenantDto>.Create(result.Items, result.PageNumber, pageSize, result.TotalCount));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TenantDto>>> GetTenant(Guid id)
    {
        var result = await Mediator.Send(new GetTenantByIdQuery(id));
        return Ok(ApiResponse<TenantDto>.Success(result));
    }

    [HttpPatch("{id:guid}/suspend")]
    public async Task<ActionResult<ApiResponse<object>>> Suspend(Guid id)
    {
        await Mediator.Send(new SuspendTenantCommand(id));
        return Ok(ApiResponse<object>.Success(new { }, "Tenant suspended."));
    }

    [HttpPatch("{id:guid}/reactivate")]
    public async Task<ActionResult<ApiResponse<object>>> Reactivate(Guid id)
    {
        await Mediator.Send(new ReactivateTenantCommand(id));
        return Ok(ApiResponse<object>.Success(new { }, "Tenant reactivated."));
    }
}
