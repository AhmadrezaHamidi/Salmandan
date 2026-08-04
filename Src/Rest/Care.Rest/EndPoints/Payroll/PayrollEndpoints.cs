using System.Security.Claims;
using Care.Application.Contract.Services;
using Care.Rest.EndPoints.Payroll;

namespace Care.Rest.EndPoints;

/// <summary>
/// Endpoints حقوق پرستار و سود مؤسسه.
/// داده‌ی مالی داخلی است — علاوه بر نقش، مالکیت مؤسسه هم بررسی می‌شود.
/// </summary>
public class PayrollEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(PayrollConstants.Routes.BaseRoute)
            .WithApiVersionSet()
            .WithTags(PayrollConstants.Tag)
            .RequireAuthorization();

        group.MapPostEndpoint<GeneratePayrollResponse>(PayrollConstants.Routes.Generate, PostGenerate, PayrollConstants.Names.Generate, PayrollConstants.Docs.Generate.Summary, PayrollConstants.Docs.Generate.Description).RequireRoles(CareRoles.AgencyStaff);
        group.MapPutEndpoint(                          PayrollConstants.Routes.MarkPaid, PutPaid,     PayrollConstants.Names.MarkPaid, PayrollConstants.Docs.MarkPaid.Summary, PayrollConstants.Docs.MarkPaid.Description).RequireRoles(CareRoles.AgencyStaff);
        group.MapPutEndpoint(                          PayrollConstants.Routes.Cancel,   PutCancel,   PayrollConstants.Names.Cancel,   PayrollConstants.Docs.Cancel.Summary,   PayrollConstants.Docs.Cancel.Description)  .RequireRoles(CareRoles.AgencyStaff);

        group.MapGetEndpoint<IReadOnlyList<PayrollPreviewItem>>(PayrollConstants.Routes.Preview,  GetPreview, PayrollConstants.Names.Preview,  PayrollConstants.Docs.Preview.Summary,  PayrollConstants.Docs.Preview.Description) .RequireRoles(CareRoles.AgencyStaff);
        group.MapGetEndpoint<AgencyProfitResult>(               PayrollConstants.Routes.Profit,   GetProfit,  PayrollConstants.Names.Profit,   PayrollConstants.Docs.Profit.Summary,   PayrollConstants.Docs.Profit.Description)  .RequireRoles(CareRoles.AgencyStaff);
        group.MapGetEndpoint<PagedResult<PayoutListItem>>(      PayrollConstants.Routes.GetPaged, Gets,       PayrollConstants.Names.GetPaged, PayrollConstants.Docs.GetPaged.Summary, PayrollConstants.Docs.GetPaged.Description).RequireRoles(CareRoles.AgencyStaff);

        // پرستار هم باید فیش خودش را ببیند — مالکیت داخل endpoint بررسی می‌شود
        group.MapGetEndpoint<PayoutResult>(PayrollConstants.Routes.GetById, Get, PayrollConstants.Names.GetById, PayrollConstants.Docs.GetById.Summary, PayrollConstants.Docs.GetById.Description);
    }

    private static async Task<GeneratePayrollResponse> PostGenerate(
        [FromBody] GeneratePayrollCommand cmd, IUserContext user, ClaimsPrincipal principal,
        ICareAccessGuard guard, ICommandBus bus, CancellationToken ct)
    {
        await guard.EnsureOwnsAgencyAsync(cmd.AgencyId, user.UserId, principal.RoleOf(), ct);
        return await bus.Dispatch<GeneratePayrollResponse>(cmd, ct);
    }

    private static async Task<bool> PutPaid(long id, [FromBody] MarkPayoutPaidCommand cmd, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(cmd with { PayoutId = id }, ct);

    private static async Task<bool> PutCancel(long id, [FromBody] CancelPayoutCommand cmd, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(cmd with { PayoutId = id }, ct);

    private static async Task<IReadOnlyList<PayrollPreviewItem>> GetPreview(
        [AsParameters] PreviewPayrollQuery query, IUserContext user, ClaimsPrincipal principal,
        ICareAccessGuard guard, IQueryBus bus, CancellationToken ct)
    {
        await guard.EnsureOwnsAgencyAsync(query.AgencyId, user.UserId, principal.RoleOf(), ct);
        return await bus.DispatchAsync<IReadOnlyList<PayrollPreviewItem>>(query, ct);
    }

    private static async Task<AgencyProfitResult> GetProfit(
        [AsParameters] GetAgencyProfitQuery query, IUserContext user, ClaimsPrincipal principal,
        ICareAccessGuard guard, IQueryBus bus, CancellationToken ct)
    {
        await guard.EnsureOwnsAgencyAsync(query.AgencyId, user.UserId, principal.RoleOf(), ct);
        return await bus.DispatchAsync<AgencyProfitResult>(query, ct);
    }

    private static async Task<PagedResult<PayoutListItem>> Gets(
        [AsParameters] GetPayoutsQuery query, IUserContext user, ClaimsPrincipal principal,
        ICareAccessGuard guard, IQueryBus bus, CancellationToken ct)
    {
        // بدون شناسه مؤسسه، لیست همه‌ی مؤسسات برمی‌گشت — پس اجباری است
        await guard.EnsureOwnsAgencyAsync(query.AgencyId ?? 0, user.UserId, principal.RoleOf(), ct);
        return await bus.DispatchAsync<PagedResult<PayoutListItem>>(query, ct);
    }

    private static async Task<PayoutResult?> Get(
        long id, IUserContext user, ClaimsPrincipal principal,
        ICareAccessGuard guard, IQueryBus bus, CancellationToken ct)
    {
        var payout = await bus.DispatchAsync<PayoutResult?>(new GetPayoutQuery(id), ct);
        if (payout is null) return null;

        await guard.EnsureCanViewPayoutAsync(payout.AgencyId, payout.CaregiverId, user.UserId, principal.RoleOf(), ct);
        return payout;
    }
}
