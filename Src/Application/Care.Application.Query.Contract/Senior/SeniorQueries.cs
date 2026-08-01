namespace Senior.Application.Queries;

/// <summary>پرونده کامل سالمند — ActorUserId برای کنترل دسترسی اجباری است</summary>
public record GetSeniorQuery(long Id, long ActorUserId) : IQuery<SeniorResult?>;

/// <summary>سالمندانی که کاربر جاری به آن‌ها دسترسی دارد</summary>
public record GetMySeniorsQuery(long UserId) : IQuery<IReadOnlyList<SeniorListItem>>;

/// <summary>اعضای حلقه خانواده یک سالمند</summary>
public record GetFamilyCircleQuery(long SeniorId, long ActorUserId) : IQuery<IReadOnlyList<FamilyMemberItem>>;

/// <summary>
/// خلاصه پرونده برای اورژانس — قابل تبدیل به QR/PDF.
/// ارزان برای ساخت، اثر روانی و بازاریابی بالا.
/// </summary>
public record GetEmergencyCardQuery(long SeniorId, long ActorUserId) : IQuery<EmergencyCardResult?>;

public sealed record SeniorResult(
    long     Id,
    string   FullName,
    string?  NationalId,
    DateOnly BirthDate,
    int      Age,
    string   Gender,
    string   DependencyLevel,
    string   City,
    string?  District,
    string   Address,
    double?  Latitude,
    double?  Longitude,
    string?  Notes,
    bool     IsActive,
    DateTime CreatedAt,
    IReadOnlyList<ConditionItem>        Conditions,
    IReadOnlyList<MedicationItem>       Medications,
    IReadOnlyList<AllergyItem>          Allergies,
    IReadOnlyList<EmergencyContactItem> EmergencyContacts,
    IReadOnlyList<FamilyMemberItem>     FamilyMembers);

public sealed record SeniorListItem(
    long     Id,
    string   FullName,
    int      Age,
    string   Gender,
    string   DependencyLevel,
    string   City,
    string   MyAccessLevel,
    bool     IsActive);

public sealed record ConditionItem(long Id, string Title, string? Description, DateOnly? DiagnosedAt, bool IsChronic);

public sealed record MedicationItem(long Id, string Name, string Dosage, string Frequency, string? Instruction, DateOnly StartDate, DateOnly? EndDate, bool IsActive);

public sealed record AllergyItem(long Id, string Title, string? Reaction);

public sealed record EmergencyContactItem(long Id, string FullName, string PhoneNumber, string Relation, int Priority);

public sealed record FamilyMemberItem(long UserId, string DisplayName, string PhoneNumber, string Relation, string AccessLevel, string? TimeZoneId, DateTime JoinedAt);

/// <summary>کارت اورژانس — همه‌ی آنچه یک اورژانس‌گر در ۱۰ ثانیه باید بداند</summary>
public sealed record EmergencyCardResult(
    long     SeniorId,
    string   FullName,
    int      Age,
    string   Gender,
    string   Address,
    string   DependencyLevel,
    IReadOnlyList<string>               Conditions,
    IReadOnlyList<string>               Medications,
    IReadOnlyList<string>               Allergies,
    IReadOnlyList<EmergencyContactItem> Contacts);
