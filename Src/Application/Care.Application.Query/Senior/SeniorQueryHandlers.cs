namespace Care.Application.Query.Senior;

/// <summary>
/// Query های پرونده سالمند.
/// هر Query که داده‌ی سلامت برمی‌گرداند، ابتدا عضویت کاربر در حلقه خانواده را بررسی می‌کند.
/// </summary>
public sealed class SeniorQueryHandlers(SeniorReadRepository repo) :
    IQueryHandler<GetSeniorQuery, SeniorResult?>,
    IQueryHandler<GetMySeniorsQuery, IReadOnlyList<SeniorListItem>>,
    IQueryHandler<GetFamilyCircleQuery, IReadOnlyList<FamilyMemberItem>>,
    IQueryHandler<GetEmergencyCardQuery, EmergencyCardResult?>
{
    public async Task<SeniorResult?> HandleAsync(GetSeniorQuery query, CancellationToken ct)
    {
        await EnsureAccessAsync(query.Id, query.ActorUserId, ct);
        return await repo.GetFullAsync(query.Id, ct);
    }

    public async Task<IReadOnlyList<SeniorListItem>> HandleAsync(GetMySeniorsQuery query, CancellationToken ct)
        => await repo.GetByFamilyUserAsync(query.UserId, ct);

    public async Task<IReadOnlyList<FamilyMemberItem>> HandleAsync(GetFamilyCircleQuery query, CancellationToken ct)
    {
        await EnsureAccessAsync(query.SeniorId, query.ActorUserId, ct);
        return await repo.GetFamilyCircleAsync(query.SeniorId, ct);
    }

    public async Task<EmergencyCardResult?> HandleAsync(GetEmergencyCardQuery query, CancellationToken ct)
    {
        await EnsureAccessAsync(query.SeniorId, query.ActorUserId, ct);
        return await repo.GetEmergencyCardAsync(query.SeniorId, ct);
    }

    private async Task EnsureAccessAsync(long seniorId, long userId, CancellationToken ct)
    {
        if (!await repo.HasAccessAsync(seniorId, userId, ct))
            throw new AccessDeniedException();
    }
}
