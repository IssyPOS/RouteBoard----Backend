using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSShopTicketing.Application.Mailboxes.Commands.CreateMailbox;
using POSShopTicketing.Application.Mailboxes.Commands.VerifyMailbox;
using POSShopTicketing.Application.Mailboxes.Queries.GetMailboxes;
using POSShopTicketing.Shared.Wrappers;

namespace POSShopTicketing.Api.Controllers;

/// <summary>Inbound addresses a tenant receives support email at.
/// "Admin: Manage ... mailboxes" - writes are Owner/Admin only.</summary>
public class MailboxesController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<MailboxDto>>>> GetMailboxes()
    {
        var result = await Mediator.Send(new GetMailboxesQuery());
        return Ok(ApiResponse<List<MailboxDto>>.Success(result));
    }

    [Authorize(Roles = "Owner,Admin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateMailbox(CreateMailboxCommand command)
    {
        var id = await Mediator.Send(command);
        return Ok(ApiResponse<Guid>.Success(id, "Mailbox created. Configure your DNS/forwarding, then call verify."));
    }

    [Authorize(Roles = "Owner,Admin")]
    [HttpPost("{id:guid}/verify")]
    public async Task<ActionResult<ApiResponse<object>>> Verify(Guid id)
    {
        await Mediator.Send(new VerifyMailboxCommand(id));
        return Ok(ApiResponse<object>.Success(new { }, "Mailbox verified."));
    }
}
