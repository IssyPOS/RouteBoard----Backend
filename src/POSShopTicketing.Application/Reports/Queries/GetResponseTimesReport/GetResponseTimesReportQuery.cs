using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Interfaces;

namespace POSShopTicketing.Application.Reports.Queries.GetResponseTimesReport;

/// <summary>GET /reports/response-times - average first-response and
/// resolution time (in minutes), overall and per assigned agent, for
/// tickets created in the requested window.</summary>
public record GetResponseTimesReportQuery(DateTime? From = null, DateTime? To = null) : IRequest<ResponseTimesReportDto>;

public record AgentResponseTimeDto(
    Guid TeamMemberId, string TeamMemberName, int TicketCount, double? AvgFirstResponseMinutes, double? AvgResolutionMinutes);

public record ResponseTimesReportDto(
    DateTime From, DateTime To,
    double? OverallAvgFirstResponseMinutes, double? OverallAvgResolutionMinutes,
    List<AgentResponseTimeDto> ByAgent);

public class GetResponseTimesReportQueryHandler : IRequestHandler<GetResponseTimesReportQuery, ResponseTimesReportDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;

    public GetResponseTimesReportQueryHandler(IApplicationDbContext context, IDateTime dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<ResponseTimesReportDto> Handle(GetResponseTimesReportQuery request, CancellationToken cancellationToken)
    {
        var to = request.To ?? _dateTime.Now;
        var from = request.From ?? to.AddDays(-30);

        var tickets = await _context.Tickets
            .Include(t => t.AssignedToTeamMember)
            .AsNoTracking()
            .Where(t => t.CreatedAt >= from && t.CreatedAt <= to)
            .Select(t => new
            {
                t.CreatedAt,
                t.FirstResponseAt,
                t.ResolvedAt,
                t.AssignedToTeamMemberId,
                AssignedToTeamMemberName = t.AssignedToTeamMember != null ? t.AssignedToTeamMember.FirstName + " " + t.AssignedToTeamMember.LastName : null
            })
            .ToListAsync(cancellationToken);

        double? Avg(IEnumerable<double> values)
        {
            var list = values.ToList();
            return list.Count == 0 ? null : list.Average();
        }

        var firstResponseMinutes = tickets
            .Where(t => t.FirstResponseAt.HasValue)
            .Select(t => (t.FirstResponseAt!.Value - t.CreatedAt).TotalMinutes);

        var resolutionMinutes = tickets
            .Where(t => t.ResolvedAt.HasValue)
            .Select(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalMinutes);

        var byAgent = tickets
            .Where(t => t.AssignedToTeamMemberId.HasValue)
            .GroupBy(t => new { t.AssignedToTeamMemberId, t.AssignedToTeamMemberName })
            .Select(g => new AgentResponseTimeDto(
                g.Key.AssignedToTeamMemberId!.Value,
                g.Key.AssignedToTeamMemberName ?? "(unknown)",
                g.Count(),
                Avg(g.Where(t => t.FirstResponseAt.HasValue).Select(t => (t.FirstResponseAt!.Value - t.CreatedAt).TotalMinutes)),
                Avg(g.Where(t => t.ResolvedAt.HasValue).Select(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalMinutes))))
            .OrderByDescending(a => a.TicketCount)
            .ToList();

        return new ResponseTimesReportDto(from, to, Avg(firstResponseMinutes), Avg(resolutionMinutes), byAgent);
    }
}
