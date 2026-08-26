using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.Reports.Queries.GetVolumeReport;

/// <summary>GET /reports/volume - tickets created and resolved per day
/// over the requested window (defaults to the last 30 days).</summary>
public record GetVolumeReportQuery(DateTime? From = null, DateTime? To = null) : IRequest<VolumeReportDto>;

public record VolumeReportDayDto(DateOnly Date, int Created, int Resolved);

public record VolumeReportDto(
    DateTime From, DateTime To, int TotalCreated, int TotalResolved, List<VolumeReportDayDto> Days);

public class GetVolumeReportQueryHandler : IRequestHandler<GetVolumeReportQuery, VolumeReportDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;

    public GetVolumeReportQueryHandler(IApplicationDbContext context, IDateTime dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<VolumeReportDto> Handle(GetVolumeReportQuery request, CancellationToken cancellationToken)
    {
        var to = request.To ?? _dateTime.Now;
        var from = request.From ?? to.AddDays(-30);

        var created = await _context.Tickets
            .AsNoTracking()
            .Where(t => t.CreatedAt >= from && t.CreatedAt <= to)
            .Select(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        var resolved = await _context.Tickets
            .AsNoTracking()
            .Where(t => t.ResolvedAt != null && t.ResolvedAt >= from && t.ResolvedAt <= to)
            .Select(t => t.ResolvedAt!.Value)
            .ToListAsync(cancellationToken);

        var createdByDay = created.GroupBy(d => DateOnly.FromDateTime(d)).ToDictionary(g => g.Key, g => g.Count());
        var resolvedByDay = resolved.GroupBy(d => DateOnly.FromDateTime(d)).ToDictionary(g => g.Key, g => g.Count());

        var days = new List<VolumeReportDayDto>();
        for (var day = DateOnly.FromDateTime(from); day <= DateOnly.FromDateTime(to); day = day.AddDays(1))
        {
            days.Add(new VolumeReportDayDto(day, createdByDay.GetValueOrDefault(day), resolvedByDay.GetValueOrDefault(day)));
        }

        return new VolumeReportDto(from, to, created.Count, resolved.Count, days);
    }
}
