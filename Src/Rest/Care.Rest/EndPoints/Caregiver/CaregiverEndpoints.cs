using Care.Domain.Caregiver.Enums;
using Care.Rest.EndPoints.Caregiver;

namespace Care.Rest.EndPoints;

/// <summary>Endpoints مدیریت پرستاران و احراز صلاحیت</summary>
public class CaregiverEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(CaregiverConstants.Routes.BaseRoute)
            .WithApiVersionSet()
            .WithTags(CaregiverConstants.Tag)
            .RequireAuthorization();

        group.MapPostEndpoint<long>(  CaregiverConstants.Routes.Register,  Post,          CaregiverConstants.Names.Register,  CaregiverConstants.Docs.Register.Summary,  CaregiverConstants.Docs.Register.Description);
        group.MapPutEndpoint(         CaregiverConstants.Routes.Documents, PutDocuments,  CaregiverConstants.Names.Documents, CaregiverConstants.Docs.Documents.Summary, CaregiverConstants.Docs.Documents.Description);
        group.MapPutEndpoint(         CaregiverConstants.Routes.Verify,    PutVerify,     CaregiverConstants.Names.Verify,    CaregiverConstants.Docs.Verify.Summary,    CaregiverConstants.Docs.Verify.Description);
        group.MapPutEndpoint(         CaregiverConstants.Routes.Reject,    PutReject,     CaregiverConstants.Names.Reject,    CaregiverConstants.Docs.Reject.Summary,    CaregiverConstants.Docs.Reject.Description);
        group.MapPutEndpoint(         CaregiverConstants.Routes.Suspend,   PutSuspend,    CaregiverConstants.Names.Suspend,   CaregiverConstants.Docs.Suspend.Summary,   CaregiverConstants.Docs.Suspend.Description);
        group.MapPutEndpoint(         CaregiverConstants.Routes.Update,    PutUpdate,     CaregiverConstants.Names.Update,    CaregiverConstants.Docs.Update.Summary,    CaregiverConstants.Docs.Update.Description);
        group.MapPostEndpoint<bool>(  CaregiverConstants.Routes.AddSkill,  PostSkill,     CaregiverConstants.Names.AddSkill,  CaregiverConstants.Docs.AddSkill.Summary,  CaregiverConstants.Docs.AddSkill.Description);
        group.MapDeleteEndpoint<bool>(CaregiverConstants.Routes.RemoveSkill, DeleteSkill, CaregiverConstants.Names.RemoveSkill, CaregiverConstants.Docs.RemoveSkill.Summary, CaregiverConstants.Docs.RemoveSkill.Description);

        group.MapGetEndpoint<CaregiverResult>(                  CaregiverConstants.Routes.GetById,   Get,          CaregiverConstants.Names.GetById,   CaregiverConstants.Docs.GetById.Summary,   CaregiverConstants.Docs.GetById.Description);
        group.MapGetEndpoint<PagedResult<CaregiverListItem>>(   CaregiverConstants.Routes.GetPaged,  Gets,         CaregiverConstants.Names.GetPaged,  CaregiverConstants.Docs.GetPaged.Summary,  CaregiverConstants.Docs.GetPaged.Description);
        group.MapGetEndpoint<IReadOnlyList<CaregiverListItem>>( CaregiverConstants.Routes.Available, GetAvailable, CaregiverConstants.Names.Available, CaregiverConstants.Docs.Available.Summary, CaregiverConstants.Docs.Available.Description);
    }

    private static async Task<long> Post(long agencyId, [FromBody] RegisterCaregiverCommand cmd, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<long>(cmd with { AgencyId = agencyId }, ct);

    private static async Task<bool> PutDocuments(long id, [FromBody] SubmitCaregiverDocumentsCommand cmd, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(cmd with { CaregiverId = id }, ct);

    private static async Task<bool> PutVerify(long id, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(new VerifyCaregiverCommand(id), ct);

    private static async Task<bool> PutReject(long id, [FromBody] RejectCaregiverCommand cmd, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(cmd with { CaregiverId = id }, ct);

    private static async Task<bool> PutSuspend(long id, [FromBody] SuspendCaregiverCommand cmd, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(cmd with { CaregiverId = id }, ct);

    private static async Task<bool> PutUpdate(long id, [FromBody] UpdateCaregiverProfileCommand cmd, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(cmd with { CaregiverId = id }, ct);

    private static async Task<bool> PostSkill(long id, [FromBody] AddCaregiverSkillCommand cmd, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(cmd with { CaregiverId = id }, ct);

    private static async Task<bool> DeleteSkill(long id, CaregiverSkillType skillType, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(new RemoveCaregiverSkillCommand(id, skillType), ct);

    private static async Task<CaregiverResult?> Get(long id, IQueryBus bus, CancellationToken ct)
        => await bus.DispatchAsync<CaregiverResult?>(new GetCaregiverQuery(id), ct);

    private static async Task<PagedResult<CaregiverListItem>> Gets([AsParameters] GetCaregiversQuery query, IQueryBus bus, CancellationToken ct)
        => await bus.DispatchAsync<PagedResult<CaregiverListItem>>(query, ct);

    private static async Task<IReadOnlyList<CaregiverListItem>> GetAvailable([AsParameters] GetAvailableCaregiversQuery query, IQueryBus bus, CancellationToken ct)
        => await bus.DispatchAsync<IReadOnlyList<CaregiverListItem>>(query, ct);
}
