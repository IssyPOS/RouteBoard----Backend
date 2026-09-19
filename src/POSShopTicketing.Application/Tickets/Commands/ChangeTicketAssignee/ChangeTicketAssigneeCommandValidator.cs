using FluentValidation;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.Tickets.Commands.ChangeTicketAssignee;

public class ChangeTicketAssigneeCommandValidator
    : AbstractValidator<ChangeTicketAssigneeCommand>
{
    private readonly IApplicationDbContext _context;

    public ChangeTicketAssigneeCommandValidator(
        IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.TicketId)
            .NotEmpty();

        RuleFor(v => v.AssignedToTeamMemberId)
            .MustAsync(BeActiveTeamMember)
            .WithMessage("Assigned team member is disabled or has not accepted their invitation.");
    }

    private async Task<bool> BeActiveTeamMember(
        Guid? teamMemberId,
        CancellationToken cancellationToken)
    {
        if (!teamMemberId.HasValue)
            return true;

        return await _context.TeamMembers
            .AnyAsync(
                x => x.Id == teamMemberId.Value &&
                     x.Status == TeamMemberStatus.Active,
                cancellationToken);
    }
}