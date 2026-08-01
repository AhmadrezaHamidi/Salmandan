namespace Care.Application.Query.Billing;

public sealed class BillingQueryHandlers(BillingReadRepository repo) :
    IQueryHandler<GetOrderQuery, OrderResult?>,
    IQueryHandler<GetOrderByShiftQuery, OrderResult?>,
    IQueryHandler<GetOrdersQuery, PagedResult<OrderListItem>>,
    IQueryHandler<GetAgencyBalanceQuery, AgencyBalanceResult>
{
    public async Task<OrderResult?> HandleAsync(GetOrderQuery query, CancellationToken ct)
        => await repo.GetByIdAsync(query.Id, ct);

    public async Task<OrderResult?> HandleAsync(GetOrderByShiftQuery query, CancellationToken ct)
        => await repo.GetByShiftIdAsync(query.ShiftId, ct);

    public async Task<PagedResult<OrderListItem>> HandleAsync(GetOrdersQuery query, CancellationToken ct)
    {
        var (items, total) = await repo.GetPagedAsync(
            query.Page, query.PageSize, query.SeniorId, query.AgencyId, query.Status, ct);

        return new PagedResult<OrderListItem>(items, total, query.Page, query.PageSize);
    }

    public async Task<AgencyBalanceResult> HandleAsync(GetAgencyBalanceQuery query, CancellationToken ct)
        => await repo.GetAgencyBalanceAsync(query.AgencyId, ct);
}
