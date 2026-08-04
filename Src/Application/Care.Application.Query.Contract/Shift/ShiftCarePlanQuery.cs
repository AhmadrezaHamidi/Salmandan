using Senior.Application.Queries;

namespace Shift.Application.Queries;

/// <summary>
/// پرونده مراقبت سالمند برای پرستارِ همان شیفت.
///
/// پرستار حین شیفت باید داروها، بیماری‌ها، حساسیت‌ها و تماس اضطراری را ببیند،
/// ولی نباید به کل پرونده‌ی خانواده دسترسی داشته باشد. دسترسی به همین شیفت گره خورده
/// و با پایان شیفت قطع می‌شود.
/// </summary>
public record GetShiftCarePlanQuery(long ShiftId, long ActorUserId) : IQuery<ShiftCarePlanResult?>;

public sealed record ShiftCarePlanResult(
    long     ShiftId,
    long     SeniorId,
    string   SeniorFullName,
    int      Age,
    string   Gender,
    string   DependencyLevel,
    string   Address,
    string?  Notes,
    IReadOnlyList<ConditionItem>        Conditions,
    IReadOnlyList<MedicationItem>       Medications,
    IReadOnlyList<AllergyItem>          Allergies,
    IReadOnlyList<EmergencyContactItem> EmergencyContacts);
