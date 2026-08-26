using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSShopTicketing.Application.Reports.Queries.GetResponseTimesReport;
using POSShopTicketing.Application.Reports.Queries.GetVolumeReport;
using POSShopTicketing.Shared.Wrappers;

namespace POSShopTicketing.Api.Controllers;

/// <summary>"Manager: ...view reports" - Manager+ only, matching the roles table.</summary>
[Authorize(Roles = "Manager,Admin,Owner")]
public class ReportsController : ApiControllerBase
{
    [HttpGet("volume")]
    public async Task<ActionResult<ApiResponse<VolumeReportDto>>> GetVolume(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var result = await Mediator.Send(new GetVolumeReportQuery(from, to));
        return Ok(ApiResponse<VolumeReportDto>.Success(result));
    }

    [HttpGet("response-times")]
    public async Task<ActionResult<ApiResponse<ResponseTimesReportDto>>> GetResponseTimes(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var result = await Mediator.Send(new GetResponseTimesReportQuery(from, to));
        return Ok(ApiResponse<ResponseTimesReportDto>.Success(result));
    }
}
