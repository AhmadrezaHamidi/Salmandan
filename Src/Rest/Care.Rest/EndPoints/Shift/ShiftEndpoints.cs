using Care.Rest.EndPoints.Shift;

namespace Care.Rest.EndPoints;

/// <summary>Endpoints چرخه حیات شیفت مراقبت</summary>
public class ShiftEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ShiftConstants.Routes.BaseRoute)
            .WithApiVersionSet()
            .WithTags(ShiftConstants.Tag)
            .RequireAuthorization();

        // خانواده شیفت درخواست می‌دهد
        group.MapPostEndpoint<long>(ShiftConstants.Routes.Request, Post, ShiftConstants.Names.Request, ShiftConstants.Docs.Request.Summary, ShiftConstants.Docs.Request.Description).RequireRoles(CareRoles.FamilyOrAdmin);

        // مؤسسه تخصیص می‌دهد و شیفت را می‌بندد
        group.MapPutEndpoint(       ShiftConstants.Routes.Assign,   PutAssign,   ShiftConstants.Names.Assign,   ShiftConstants.Docs.Assign.Summary,   ShiftConstants.Docs.Assign.Description)  .RequireRoles(CareRoles.AgencyStaff);
        group.MapPutEndpoint(       ShiftConstants.Routes.Complete, PutComplete, ShiftConstants.Names.Complete, ShiftConstants.Docs.Complete.Summary, ShiftConstants.Docs.Complete.Description).RequireRoles(CareRoles.AgencyStaff);
        group.MapPutEndpoint(       ShiftConstants.Routes.NoShow,   PutNoShow,   ShiftConstants.Names.NoShow,   ShiftConstants.Docs.NoShow.Summary,   ShiftConstants.Docs.NoShow.Description)  .RequireRoles(CareRoles.AgencyStaff);
        group.MapPostEndpoint<long>(ShiftConstants.Routes.Replace,  PostReplace, ShiftConstants.Names.Replace,  ShiftConstants.Docs.Replace.Summary,  ShiftConstants.Docs.Replace.Description) .RequireRoles(CareRoles.AgencyStaff);

        // فقط پرستار می‌تواند ورود و خروج بزند
        group.MapPutEndpoint(ShiftConstants.Routes.CheckIn,  PutCheckIn,  ShiftConstants.Names.CheckIn,  ShiftConstants.Docs.CheckIn.Summary,  ShiftConstants.Docs.CheckIn.Description) .RequireRoles(CareRoles.CaregiverOrAdmin);
        group.MapPutEndpoint(ShiftConstants.Routes.CheckOut, PutCheckOut, ShiftConstants.Names.CheckOut, ShiftConstants.Docs.CheckOut.Summary, ShiftConstants.Docs.CheckOut.Description).RequireRoles(CareRoles.CaregiverOrAdmin);

        // لغو از هر دو طرف ممکن است
        group.MapPutEndpoint(ShiftConstants.Routes.Cancel, PutCancel, ShiftConstants.Names.Cancel, ShiftConstants.Docs.Cancel.Summary, ShiftConstants.Docs.Cancel.Description).RequireRoles(CareRoles.FamilyOrAgency);

        group.MapGetEndpoint<ShiftResult>(                 ShiftConstants.Routes.GetById,  Get,         ShiftConstants.Names.GetById,  ShiftConstants.Docs.GetById.Summary,  ShiftConstants.Docs.GetById.Description);
        group.MapGetEndpoint<PagedResult<ShiftListItem>>(  ShiftConstants.Routes.GetPaged, Gets,        ShiftConstants.Names.GetPaged, ShiftConstants.Docs.GetPaged.Summary, ShiftConstants.Docs.GetPaged.Description);
        group.MapGetEndpoint<IReadOnlyList<ShiftListItem>>(ShiftConstants.Routes.Calendar, GetCalendar, ShiftConstants.Names.Calendar, ShiftConstants.Docs.Calendar.Summary, ShiftConstants.Docs.Calendar.Description);

        // داشبورد عملیاتی مؤسسه
        group.MapGetEndpoint<IReadOnlyList<CoverageRiskItem>>(ShiftConstants.Routes.CoverageRisk, GetCoverageRisk, ShiftConstants.Names.CoverageRisk, ShiftConstants.Docs.CoverageRisk.Summary, ShiftConstants.Docs.CoverageRisk.Description).RequireRoles(CareRoles.AgencyStaff);
    }

    private static async Task<long> Post([FromBody] RequestShiftCommand cmd, IUserContext user, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<long>(cmd with { RequestedByUserId = user.UserId }, ct);

    private static async Task<bool> PutAssign(long id, [FromBody] AssignCaregiverCommand cmd, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(cmd with { ShiftId = id }, ct);

    private static async Task<bool> PutCheckIn(long id, [FromBody] CheckInShiftCommand cmd, IUserContext user, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(cmd with { ShiftId = id, ActorUserId = user.UserId }, ct);

    private static async Task<bool> PutCheckOut(long id, IUserContext user, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(new CheckOutShiftCommand(id, user.UserId), ct);

    private static async Task<bool> PutComplete(long id, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(new CompleteShiftCommand(id), ct);

    private static async Task<bool> PutCancel(long id, [FromBody] CancelShiftCommand cmd, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(cmd with { ShiftId = id }, ct);

    private static async Task<bool> PutNoShow(long id, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(new MarkShiftNoShowCommand(id), ct);

    private static async Task<long> PostReplace(long id, [FromBody] ReplaceShiftCommand cmd, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<long>(cmd with { ShiftId = id }, ct);

    private static async Task<ShiftResult?> Get(long id, IQueryBus bus, CancellationToken ct)
        => await bus.DispatchAsync<ShiftResult?>(new GetShiftQuery(id), ct);

    private static async Task<PagedResult<ShiftListItem>> Gets([AsParameters] GetShiftsQuery query, IQueryBus bus, CancellationToken ct)
        => await bus.DispatchAsync<PagedResult<ShiftListItem>>(query, ct);

    private static async Task<IReadOnlyList<ShiftListItem>> GetCalendar([AsParameters] GetShiftCalendarQuery query, IQueryBus bus, CancellationToken ct)
        => await bus.DispatchAsync<IReadOnlyList<ShiftListItem>>(query, ct);

    private static async Task<IReadOnlyList<CoverageRiskItem>> GetCoverageRisk([AsParameters] GetCoverageRiskQuery query, IQueryBus bus, CancellationToken ct)
        => await bus.DispatchAsync<IReadOnlyList<CoverageRiskItem>>(query, ct);
}
