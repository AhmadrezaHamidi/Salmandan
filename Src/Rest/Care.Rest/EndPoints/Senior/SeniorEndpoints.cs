using Care.Rest.EndPoints.Senior;

namespace Care.Rest.EndPoints;

/// <summary>
/// Endpoints پرونده مراقبت سالمند و حلقه خانواده.
/// شناسه کاربر جاری از توکن خوانده می‌شود — کنترل دسترسی در لایه Application انجام می‌شود.
/// </summary>
public class SeniorEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(SeniorConstants.Routes.BaseRoute)
            .WithApiVersionSet()
            .WithTags(SeniorConstants.Tag)
            .RequireAuthorization();

        // پرونده سالمند داده سلامت است — فقط خانواده و ادمین.
        // کنترل دقیق‌تر (عضویت در حلقه خانواده) در لایه Application انجام می‌شود.
        group.MapPostEndpoint<long>(SeniorConstants.Routes.Create,     Post,          SeniorConstants.Names.Create,     SeniorConstants.Docs.Create.Summary,     SeniorConstants.Docs.Create.Description)    .RequireRoles(CareRoles.FamilyOrAdmin);
        group.MapPutEndpoint(       SeniorConstants.Routes.Update,     PutUpdate,     SeniorConstants.Names.Update,     SeniorConstants.Docs.Update.Summary,     SeniorConstants.Docs.Update.Description)    .RequireRoles(CareRoles.FamilyOrAdmin);
        group.MapPutEndpoint(       SeniorConstants.Routes.Dependency, PutDependency, SeniorConstants.Names.Dependency, SeniorConstants.Docs.Dependency.Summary, SeniorConstants.Docs.Dependency.Description).RequireRoles(CareRoles.FamilyOrAdmin);

        group.MapPostEndpoint<bool>(  SeniorConstants.Routes.AddFamily,    PostFamily,   SeniorConstants.Names.AddFamily,    SeniorConstants.Docs.AddFamily.Summary,    SeniorConstants.Docs.AddFamily.Description)   .RequireRoles(CareRoles.FamilyOrAdmin);
        group.MapDeleteEndpoint<bool>(SeniorConstants.Routes.RemoveFamily, DeleteFamily, SeniorConstants.Names.RemoveFamily, SeniorConstants.Docs.RemoveFamily.Summary, SeniorConstants.Docs.RemoveFamily.Description).RequireRoles(CareRoles.FamilyOrAdmin);
        group.MapPutEndpoint(         SeniorConstants.Routes.ChangeAccess, PutAccess,    SeniorConstants.Names.ChangeAccess, SeniorConstants.Docs.ChangeAccess.Summary, SeniorConstants.Docs.ChangeAccess.Description).RequireRoles(CareRoles.FamilyOrAdmin);

        group.MapPostEndpoint<bool>(  SeniorConstants.Routes.AddCondition,    PostCondition,    SeniorConstants.Names.AddCondition,    SeniorConstants.Docs.AddCondition.Summary,    SeniorConstants.Docs.AddCondition.Description)   .RequireRoles(CareRoles.FamilyOrAdmin);
        group.MapDeleteEndpoint<bool>(SeniorConstants.Routes.RemoveCondition, DeleteCondition,  SeniorConstants.Names.RemoveCondition, SeniorConstants.Docs.RemoveCondition.Summary, SeniorConstants.Docs.RemoveCondition.Description).RequireRoles(CareRoles.FamilyOrAdmin);
        group.MapPostEndpoint<bool>(  SeniorConstants.Routes.AddMedication,   PostMedication,   SeniorConstants.Names.AddMedication,   SeniorConstants.Docs.AddMedication.Summary,   SeniorConstants.Docs.AddMedication.Description)  .RequireRoles(CareRoles.FamilyOrAdmin);
        group.MapDeleteEndpoint<bool>(SeniorConstants.Routes.StopMedication,  DeleteMedication, SeniorConstants.Names.StopMedication,  SeniorConstants.Docs.StopMedication.Summary,  SeniorConstants.Docs.StopMedication.Description) .RequireRoles(CareRoles.FamilyOrAdmin);
        group.MapPostEndpoint<bool>(  SeniorConstants.Routes.AddAllergy,      PostAllergy,      SeniorConstants.Names.AddAllergy,      SeniorConstants.Docs.AddAllergy.Summary,      SeniorConstants.Docs.AddAllergy.Description)     .RequireRoles(CareRoles.FamilyOrAdmin);
        group.MapPostEndpoint<bool>(  SeniorConstants.Routes.AddContact,      PostContact,      SeniorConstants.Names.AddContact,      SeniorConstants.Docs.AddContact.Summary,      SeniorConstants.Docs.AddContact.Description)     .RequireRoles(CareRoles.FamilyOrAdmin);

        group.MapGetEndpoint<SeniorResult>(                   SeniorConstants.Routes.GetById,       Get,       SeniorConstants.Names.GetById,       SeniorConstants.Docs.GetById.Summary,       SeniorConstants.Docs.GetById.Description)      .RequireRoles(CareRoles.FamilyOrAdmin);
        group.MapGetEndpoint<IReadOnlyList<SeniorListItem>>(  SeniorConstants.Routes.GetMine,       GetMine,   SeniorConstants.Names.GetMine,       SeniorConstants.Docs.GetMine.Summary,       SeniorConstants.Docs.GetMine.Description)      .RequireRoles(CareRoles.FamilyOrAdmin);
        group.MapGetEndpoint<IReadOnlyList<FamilyMemberItem>>(SeniorConstants.Routes.FamilyCircle,  GetFamily, SeniorConstants.Names.FamilyCircle,  SeniorConstants.Docs.FamilyCircle.Summary,  SeniorConstants.Docs.FamilyCircle.Description) .RequireRoles(CareRoles.FamilyOrAdmin);
        group.MapGetEndpoint<EmergencyCardResult>(            SeniorConstants.Routes.EmergencyCard, GetCard,   SeniorConstants.Names.EmergencyCard, SeniorConstants.Docs.EmergencyCard.Summary, SeniorConstants.Docs.EmergencyCard.Description).RequireRoles(CareRoles.FamilyOrAdmin);
    }

    // ── Commands ──────────────────────────────────────────────────────────

    private static async Task<long> Post([FromBody] CreateSeniorCommand cmd, IUserContext user, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<long>(cmd with { CreatedByUserId = user.UserId }, ct);

    private static async Task<bool> PutUpdate(long id, [FromBody] UpdateSeniorCommand cmd, IUserContext user, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(cmd with { SeniorId = id, ActorUserId = user.UserId }, ct);

    private static async Task<bool> PutDependency(long id, [FromBody] ChangeDependencyLevelCommand cmd, IUserContext user, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(cmd with { SeniorId = id, ActorUserId = user.UserId }, ct);

    private static async Task<bool> PostFamily(long id, [FromBody] AddFamilyMemberCommand cmd, IUserContext user, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(cmd with { SeniorId = id, ActorUserId = user.UserId }, ct);

    private static async Task<bool> DeleteFamily(long id, long memberUserId, IUserContext user, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(new RemoveFamilyMemberCommand(id, user.UserId, memberUserId), ct);

    private static async Task<bool> PutAccess(long id, long memberUserId, [FromBody] ChangeFamilyMemberAccessCommand cmd, IUserContext user, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(cmd with { SeniorId = id, ActorUserId = user.UserId, MemberUserId = memberUserId }, ct);

    private static async Task<bool> PostCondition(long id, [FromBody] AddConditionCommand cmd, IUserContext user, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(cmd with { SeniorId = id, ActorUserId = user.UserId }, ct);

    private static async Task<bool> DeleteCondition(long id, long conditionId, IUserContext user, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(new RemoveConditionCommand(id, user.UserId, conditionId), ct);

    private static async Task<bool> PostMedication(long id, [FromBody] AddMedicationCommand cmd, IUserContext user, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(cmd with { SeniorId = id, ActorUserId = user.UserId }, ct);

    private static async Task<bool> DeleteMedication(long id, long medicationId, IUserContext user, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(new DiscontinueMedicationCommand(id, user.UserId, medicationId), ct);

    private static async Task<bool> PostAllergy(long id, [FromBody] AddAllergyCommand cmd, IUserContext user, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(cmd with { SeniorId = id, ActorUserId = user.UserId }, ct);

    private static async Task<bool> PostContact(long id, [FromBody] AddEmergencyContactCommand cmd, IUserContext user, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(cmd with { SeniorId = id, ActorUserId = user.UserId }, ct);

    // ── Queries ───────────────────────────────────────────────────────────

    private static async Task<SeniorResult?> Get(long id, IUserContext user, IQueryBus bus, CancellationToken ct)
        => await bus.DispatchAsync<SeniorResult?>(new GetSeniorQuery(id, user.UserId), ct);

    private static async Task<IReadOnlyList<SeniorListItem>> GetMine(IUserContext user, IQueryBus bus, CancellationToken ct)
        => await bus.DispatchAsync<IReadOnlyList<SeniorListItem>>(new GetMySeniorsQuery(user.UserId), ct);

    private static async Task<IReadOnlyList<FamilyMemberItem>> GetFamily(long id, IUserContext user, IQueryBus bus, CancellationToken ct)
        => await bus.DispatchAsync<IReadOnlyList<FamilyMemberItem>>(new GetFamilyCircleQuery(id, user.UserId), ct);

    private static async Task<EmergencyCardResult?> GetCard(long id, IUserContext user, IQueryBus bus, CancellationToken ct)
        => await bus.DispatchAsync<EmergencyCardResult?>(new GetEmergencyCardQuery(id, user.UserId), ct);
}
