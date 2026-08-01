using Care.Application.Contract.Services;

namespace Care.Application.Query.Report;

public sealed class ReportQueryHandlers(
    ReportReadRepository repo,
    IHealthRiskAnalyzer  riskAnalyzer) :
    IQueryHandler<GetShiftReportQuery, ShiftReportResult?>,
    IQueryHandler<GetReportByShiftQuery, ShiftReportResult?>,
    IQueryHandler<GetReportsBySeniorQuery, PagedResult<ShiftReportListItem>>,
    IQueryHandler<GetHealthTimelineQuery, IReadOnlyList<HealthTimelinePoint>>,
    IQueryHandler<GetHealthRiskQuery, HealthRiskResult>
{
    public async Task<ShiftReportResult?> HandleAsync(GetShiftReportQuery query, CancellationToken ct)
        => await repo.GetByIdAsync(query.Id, ct);

    public async Task<ShiftReportResult?> HandleAsync(GetReportByShiftQuery query, CancellationToken ct)
        => await repo.GetByShiftIdAsync(query.ShiftId, ct);

    public async Task<PagedResult<ShiftReportListItem>> HandleAsync(GetReportsBySeniorQuery query, CancellationToken ct)
    {
        var (items, total) = await repo.GetPagedBySeniorAsync(query.SeniorId, query.Page, query.PageSize, ct);
        return new PagedResult<ShiftReportListItem>(items, total, query.Page, query.PageSize);
    }

    public async Task<IReadOnlyList<HealthTimelinePoint>> HandleAsync(GetHealthTimelineQuery query, CancellationToken ct)
        => await repo.GetTimelineAsync(query.SeniorId, query.Count, ct);

    public async Task<HealthRiskResult> HandleAsync(GetHealthRiskQuery query, CancellationToken ct)
        => await riskAnalyzer.AnalyzeAsync(query.SeniorId, ct);
}
