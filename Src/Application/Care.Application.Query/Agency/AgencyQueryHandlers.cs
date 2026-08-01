namespace Care.Application.Query.Agency;

public sealed class AgencyQueryHandlers(AgencyReadRepository repo) :
    IQueryHandler<GetAgencyQuery, AgencyResult?>,
    IQueryHandler<GetAgenciesQuery, PagedResult<AgencyListItem>>,
    IQueryHandler<GetAvailableAgenciesQuery, IReadOnlyList<AgencyListItem>>,
    IQueryHandler<GetAgencyPricesQuery, IReadOnlyList<AgencyPriceItem>>
{
    public async Task<AgencyResult?> HandleAsync(GetAgencyQuery query, CancellationToken ct)
        => await repo.GetByIdAsync(query.Id, ct);

    public async Task<PagedResult<AgencyListItem>> HandleAsync(GetAgenciesQuery query, CancellationToken ct)
    {
        var (items, total) = await repo.GetPagedAsync(query.Page, query.PageSize, query.City, query.Status, ct);
        return new PagedResult<AgencyListItem>(items, total, query.Page, query.PageSize);
    }

    public async Task<IReadOnlyList<AgencyListItem>> HandleAsync(GetAvailableAgenciesQuery query, CancellationToken ct)
        => await repo.GetAvailableAsync(query.City, query.ServiceType, ct);

    public async Task<IReadOnlyList<AgencyPriceItem>> HandleAsync(GetAgencyPricesQuery query, CancellationToken ct)
        => await repo.GetPricesAsync(query.AgencyId, ct);
}
