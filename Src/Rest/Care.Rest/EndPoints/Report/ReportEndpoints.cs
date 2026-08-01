using Care.Application.Contract.Services;
using Care.Rest.EndPoints.Report;

namespace Care.Rest.EndPoints;

/// <summary>
/// Endpoints گزارش شیفت و تحلیل روند سلامت.
/// این داده‌ی ساختاریافته با گذر زمان به مزیت رقابتی اصلی پلتفرم تبدیل می‌شود.
/// </summary>
public class ShiftReportEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ReportConstants.Routes.BaseRoute)
            .WithApiVersionSet()
            .WithTags(ReportConstants.Tag)
            .RequireAuthorization();

        group.MapPostEndpoint<long>(ReportConstants.Routes.Submit,     Post,           ReportConstants.Names.Submit,     ReportConstants.Docs.Submit.Summary,     ReportConstants.Docs.Submit.Description);
        group.MapPutEndpoint(       ReportConstants.Routes.Medication, PutMedication,  ReportConstants.Names.Medication, ReportConstants.Docs.Medication.Summary, ReportConstants.Docs.Medication.Description);
        group.MapPutEndpoint(       ReportConstants.Routes.Incident,   PutIncident,    ReportConstants.Names.Incident,   ReportConstants.Docs.Incident.Summary,   ReportConstants.Docs.Incident.Description);
        group.MapPutEndpoint(       ReportConstants.Routes.Approve,    PutApprove,     ReportConstants.Names.Approve,    ReportConstants.Docs.Approve.Summary,    ReportConstants.Docs.Approve.Description);

        group.MapGetEndpoint<ShiftReportResult>(                      ReportConstants.Routes.GetById,     Get,          ReportConstants.Names.GetById,     ReportConstants.Docs.GetById.Summary,     ReportConstants.Docs.GetById.Description);
        group.MapGetEndpoint<ShiftReportResult>(                      ReportConstants.Routes.GetByShift,  GetByShift,   ReportConstants.Names.GetByShift,  ReportConstants.Docs.GetByShift.Summary,  ReportConstants.Docs.GetByShift.Description);
        group.MapGetEndpoint<PagedResult<ShiftReportListItem>>(       ReportConstants.Routes.GetBySenior, GetBySenior,  ReportConstants.Names.GetBySenior, ReportConstants.Docs.GetBySenior.Summary, ReportConstants.Docs.GetBySenior.Description);
        group.MapGetEndpoint<IReadOnlyList<HealthTimelinePoint>>(     ReportConstants.Routes.Timeline,    GetTimeline,  ReportConstants.Names.Timeline,    ReportConstants.Docs.Timeline.Summary,    ReportConstants.Docs.Timeline.Description);
        group.MapGetEndpoint<HealthRiskResult>(                       ReportConstants.Routes.HealthRisk,  GetRisk,      ReportConstants.Names.HealthRisk,  ReportConstants.Docs.HealthRisk.Summary,  ReportConstants.Docs.HealthRisk.Description);
    }

    private static async Task<long> Post([FromBody] SubmitShiftReportCommand cmd, IUserContext user, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<long>(cmd with { ActorUserId = user.UserId }, ct);

    private static async Task<bool> PutMedication(long id, [FromBody] RecordMedicationIntakeCommand cmd, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(cmd with { ReportId = id }, ct);

    private static async Task<bool> PutIncident(long id, [FromBody] FlagIncidentCommand cmd, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(cmd with { ReportId = id }, ct);

    private static async Task<bool> PutApprove(long id, IUserContext user, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(new ApproveShiftReportCommand(id, user.UserId), ct);

    private static async Task<ShiftReportResult?> Get(long id, IQueryBus bus, CancellationToken ct)
        => await bus.DispatchAsync<ShiftReportResult?>(new GetShiftReportQuery(id), ct);

    private static async Task<ShiftReportResult?> GetByShift(long shiftId, IQueryBus bus, CancellationToken ct)
        => await bus.DispatchAsync<ShiftReportResult?>(new GetReportByShiftQuery(shiftId), ct);

    private static async Task<PagedResult<ShiftReportListItem>> GetBySenior(
        long seniorId, int page, int pageSize, IQueryBus bus, CancellationToken ct)
        => await bus.DispatchAsync<PagedResult<ShiftReportListItem>>(
            new GetReportsBySeniorQuery(seniorId, page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize), ct);

    private static async Task<IReadOnlyList<HealthTimelinePoint>> GetTimeline(
        long seniorId, int count, IQueryBus bus, CancellationToken ct)
        => await bus.DispatchAsync<IReadOnlyList<HealthTimelinePoint>>(
            new GetHealthTimelineQuery(seniorId, count <= 0 ? 14 : count), ct);

    private static async Task<HealthRiskResult> GetRisk(long seniorId, IQueryBus bus, CancellationToken ct)
        => await bus.DispatchAsync<HealthRiskResult>(new GetHealthRiskQuery(seniorId), ct);
}
