namespace Care.Application.Query.Caregiver;

public sealed class CaregiverQueryHandlers(CaregiverReadRepository repo) :
    IQueryHandler<GetCaregiverQuery, CaregiverResult?>,
    IQueryHandler<GetCaregiversQuery, PagedResult<CaregiverListItem>>,
    IQueryHandler<GetAvailableCaregiversQuery, IReadOnlyList<CaregiverListItem>>
{
    public async Task<CaregiverResult?> HandleAsync(GetCaregiverQuery query, CancellationToken ct)
        => await repo.GetByIdAsync(query.Id, ct);

    public async Task<PagedResult<CaregiverListItem>> HandleAsync(GetCaregiversQuery query, CancellationToken ct)
    {
        var (items, total) = await repo.GetPagedAsync(query.Page, query.PageSize, query.AgencyId, query.Status, ct);
        return new PagedResult<CaregiverListItem>(items, total, query.Page, query.PageSize);
    }

    public async Task<IReadOnlyList<CaregiverListItem>> HandleAsync(GetAvailableCaregiversQuery query, CancellationToken ct)
        => await repo.GetAvailableAsync(query.AgencyId, query.Date, query.StartTime, query.EndTime, ct);
}
