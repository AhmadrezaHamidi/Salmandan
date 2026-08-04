using Care.Read.Dapper.Payroll;

namespace Care.Application.Query.Payroll;

public sealed class PayrollQueryHandlers(PayrollReadRepository repo) :
    IQueryHandler<GetPayoutQuery, PayoutResult?>,
    IQueryHandler<GetPayoutsQuery, PagedResult<PayoutListItem>>,
    IQueryHandler<PreviewPayrollQuery, IReadOnlyList<PayrollPreviewItem>>,
    IQueryHandler<GetAgencyProfitQuery, AgencyProfitResult>
{
    public async Task<PayoutResult?> HandleAsync(GetPayoutQuery query, CancellationToken ct)
        => await repo.GetByIdAsync(query.Id, ct);

    public async Task<PagedResult<PayoutListItem>> HandleAsync(GetPayoutsQuery query, CancellationToken ct)
    {
        var (items, total) = await repo.GetPagedAsync(
            query.Page, query.PageSize, query.AgencyId, query.CaregiverId, query.Status, ct);

        return new PagedResult<PayoutListItem>(items, total, query.Page, query.PageSize);
    }

    public async Task<IReadOnlyList<PayrollPreviewItem>> HandleAsync(PreviewPayrollQuery query, CancellationToken ct)
        => await repo.PreviewAsync(query.AgencyId, query.From, query.To, ct);

    public async Task<AgencyProfitResult> HandleAsync(GetAgencyProfitQuery query, CancellationToken ct)
        => await repo.GetProfitAsync(query.AgencyId, query.From, query.To, ct);
}
