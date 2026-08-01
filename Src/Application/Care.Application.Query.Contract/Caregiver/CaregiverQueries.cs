using Care.Domain.Caregiver.Enums;

namespace Caregiver.Application.Queries;

public record GetCaregiverQuery(long Id) : IQuery<CaregiverResult?>;

public record GetCaregiversQuery(
    int              Page = 1,
    int              PageSize = 20,
    long?            AgencyId = null,
    CaregiverStatus? Status = null) : IQuery<PagedResult<CaregiverListItem>>;

/// <summary>پرستاران آزاد یک مؤسسه در بازه خواسته‌شده — پایه‌ی جایگزین اضطراری</summary>
public record GetAvailableCaregiversQuery(
    long     AgencyId,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime) : IQuery<IReadOnlyList<CaregiverListItem>>;

public sealed record CaregiverResult(
    long     Id,
    long     UserId,
    long     AgencyId,
    string   AgencyName,
    string   FullName,
    string   NationalId,
    string   Gender,
    int      BirthYear,
    string   Level,
    string   Status,
    bool     HasCriminalRecordCheck,
    bool     HasCertificate,
    bool     HasLiabilityInsurance,
    decimal  RatingAvg,
    int      RatingCount,
    int      CompletedShifts,
    int      NoShowCount,
    DateTime CreatedAt);

public sealed record CaregiverListItem(
    long    Id,
    string  FullName,
    string  Gender,
    string  Level,
    string  Status,
    decimal RatingAvg,
    int     CompletedShifts);
