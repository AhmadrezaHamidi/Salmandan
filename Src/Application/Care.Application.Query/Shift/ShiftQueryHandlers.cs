namespace Care.Application.Query.Shift;

public sealed class ShiftQueryHandlers(ShiftReadRepository repo) :
    IQueryHandler<GetShiftQuery, ShiftResult?>,
    IQueryHandler<GetShiftsQuery, PagedResult<ShiftListItem>>,
    IQueryHandler<GetShiftCalendarQuery, IReadOnlyList<ShiftListItem>>,
    IQueryHandler<GetCoverageRiskQuery, IReadOnlyList<CoverageRiskItem>>
{
    public async Task<ShiftResult?> HandleAsync(GetShiftQuery query, CancellationToken ct)
        => await repo.GetByIdAsync(query.Id, ct);

    public async Task<PagedResult<ShiftListItem>> HandleAsync(GetShiftsQuery query, CancellationToken ct)
    {
        var (items, total) = await repo.GetPagedAsync(
            query.Page, query.PageSize, query.SeniorId, query.AgencyId,
            query.CaregiverId, query.Status, query.From, query.To, ct);

        return new PagedResult<ShiftListItem>(items, total, query.Page, query.PageSize);
    }

    public async Task<IReadOnlyList<ShiftListItem>> HandleAsync(GetShiftCalendarQuery query, CancellationToken ct)
        => await repo.GetCalendarAsync(query.SeniorId, query.From, query.To, ct);

    public async Task<IReadOnlyList<CoverageRiskItem>> HandleAsync(GetCoverageRiskQuery query, CancellationToken ct)
        => await repo.GetCoverageRiskAsync(query.WithinHours, ct);
}
