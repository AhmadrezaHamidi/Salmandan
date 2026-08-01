using Care.Rest.EndPoints.Agency;

namespace Care.Rest.EndPoints;

/// <summary>Endpoints مدیریت مؤسسات پرستاری — طرف عرضه پلتفرم</summary>
public class AgencyEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(AgencyConstants.Routes.BaseRoute)
            .WithApiVersionSet()
            .WithTags(AgencyConstants.Tag)
            .RequireAuthorization();

        group.MapPostEndpoint<long>(AgencyConstants.Routes.Register, Post,        AgencyConstants.Names.Register,   AgencyConstants.Docs.Register.Summary,   AgencyConstants.Docs.Register.Description);
        group.MapPutEndpoint(       AgencyConstants.Routes.Approve,  PutApprove,  AgencyConstants.Names.Approve,    AgencyConstants.Docs.Approve.Summary,    AgencyConstants.Docs.Approve.Description);
        group.MapPutEndpoint(       AgencyConstants.Routes.Reject,   PutReject,   AgencyConstants.Names.Reject,     AgencyConstants.Docs.Reject.Summary,     AgencyConstants.Docs.Reject.Description);
        group.MapPutEndpoint(       AgencyConstants.Routes.Suspend,  PutSuspend,  AgencyConstants.Names.Suspend,    AgencyConstants.Docs.Suspend.Summary,    AgencyConstants.Docs.Suspend.Description);
        group.MapPutEndpoint(       AgencyConstants.Routes.Renew,    PutRenew,    AgencyConstants.Names.Renew,      AgencyConstants.Docs.Renew.Summary,      AgencyConstants.Docs.Renew.Description);
        group.MapPutEndpoint(       AgencyConstants.Routes.Update,   PutUpdate,   AgencyConstants.Names.Update,     AgencyConstants.Docs.Update.Summary,     AgencyConstants.Docs.Update.Description);
        group.MapPutEndpoint(       AgencyConstants.Routes.Commission, PutCommission, AgencyConstants.Names.Commission, AgencyConstants.Docs.Commission.Summary, AgencyConstants.Docs.Commission.Description);
        group.MapPostEndpoint<bool>(AgencyConstants.Routes.SetPrice, PostPrice,   AgencyConstants.Names.SetPrice,   AgencyConstants.Docs.SetPrice.Summary,   AgencyConstants.Docs.SetPrice.Description);

        group.MapGetEndpoint<AgencyResult>(                    AgencyConstants.Routes.GetById,   Get,          AgencyConstants.Names.GetById,   AgencyConstants.Docs.GetById.Summary,   AgencyConstants.Docs.GetById.Description);
        group.MapGetEndpoint<PagedResult<AgencyListItem>>(     AgencyConstants.Routes.GetPaged,  Gets,         AgencyConstants.Names.GetPaged,  AgencyConstants.Docs.GetPaged.Summary,  AgencyConstants.Docs.GetPaged.Description);
        group.MapGetEndpoint<IReadOnlyList<AgencyListItem>>(   AgencyConstants.Routes.Available, GetAvailable, AgencyConstants.Names.Available, AgencyConstants.Docs.Available.Summary, AgencyConstants.Docs.Available.Description);
        group.MapGetEndpoint<IReadOnlyList<AgencyPriceItem>>(  AgencyConstants.Routes.GetPrices, GetPrices,    AgencyConstants.Names.GetPrices, AgencyConstants.Docs.GetPrices.Summary, AgencyConstants.Docs.GetPrices.Description);
        group.MapGetEndpoint<AgencyBalanceResult>(             AgencyConstants.Routes.Balance,   GetBalance,   AgencyConstants.Names.Balance,   AgencyConstants.Docs.Balance.Summary,   AgencyConstants.Docs.Balance.Description);
    }

    private static async Task<long> Post([FromBody] RegisterAgencyCommand cmd, IUserContext user, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<long>(cmd with { OwnerUserId = user.UserId }, ct);

    private static async Task<bool> PutApprove(long id, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(new ApproveAgencyCommand(id), ct);

    private static async Task<bool> PutReject(long id, [FromBody] RejectAgencyCommand cmd, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(cmd with { AgencyId = id }, ct);

    private static async Task<bool> PutSuspend(long id, [FromBody] SuspendAgencyCommand cmd, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(cmd with { AgencyId = id }, ct);

    private static async Task<bool> PutRenew(long id, [FromBody] RenewAgencyLicenseCommand cmd, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(cmd with { AgencyId = id }, ct);

    private static async Task<bool> PutUpdate(long id, [FromBody] UpdateAgencyProfileCommand cmd, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(cmd with { AgencyId = id }, ct);

    private static async Task<bool> PutCommission(long id, [FromBody] SetAgencyCommissionCommand cmd, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(cmd with { AgencyId = id }, ct);

    private static async Task<bool> PostPrice(long id, [FromBody] SetAgencyServicePriceCommand cmd, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(cmd with { AgencyId = id }, ct);

    private static async Task<AgencyResult?> Get(long id, IQueryBus bus, CancellationToken ct)
        => await bus.DispatchAsync<AgencyResult?>(new GetAgencyQuery(id), ct);

    private static async Task<PagedResult<AgencyListItem>> Gets([AsParameters] GetAgenciesQuery query, IQueryBus bus, CancellationToken ct)
        => await bus.DispatchAsync<PagedResult<AgencyListItem>>(query, ct);

    private static async Task<IReadOnlyList<AgencyListItem>> GetAvailable([AsParameters] GetAvailableAgenciesQuery query, IQueryBus bus, CancellationToken ct)
        => await bus.DispatchAsync<IReadOnlyList<AgencyListItem>>(query, ct);

    private static async Task<IReadOnlyList<AgencyPriceItem>> GetPrices(long id, IQueryBus bus, CancellationToken ct)
        => await bus.DispatchAsync<IReadOnlyList<AgencyPriceItem>>(new GetAgencyPricesQuery(id), ct);

    private static async Task<AgencyBalanceResult> GetBalance(long id, IQueryBus bus, CancellationToken ct)
        => await bus.DispatchAsync<AgencyBalanceResult>(new GetAgencyBalanceQuery(id), ct);
}
