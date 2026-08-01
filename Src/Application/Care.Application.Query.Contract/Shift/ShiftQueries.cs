using Care.Domain.Shift.Enums;

namespace Shift.Application.Queries;

public record GetShiftQuery(long Id) : IQuery<ShiftResult?>;

public record GetShiftsQuery(
    int          Page = 1,
    int          PageSize = 20,
    long?        SeniorId = null,
    long?        AgencyId = null,
    long?        CaregiverId = null,
    ShiftStatus? Status = null,
    DateOnly?    From = null,
    DateOnly?    To = null) : IQuery<PagedResult<ShiftListItem>>;

/// <summary>تقویم شیفت‌های یک سالمند در بازه تاریخ</summary>
public record GetShiftCalendarQuery(long SeniorId, DateOnly From, DateOnly To) : IQuery<IReadOnlyList<ShiftListItem>>;

/// <summary>
/// شیفت‌هایی که پرستار تخصیص نیافته و زمانشان نزدیک است.
/// داشبورد پایش تضمین پوشش شیفت (Coverage SLA).
/// </summary>
public record GetCoverageRiskQuery(int WithinHours = 24) : IQuery<IReadOnlyList<CoverageRiskItem>>;

public sealed record ShiftResult(
    long      Id,
    long      SeniorId,
    string    SeniorName,
    long      AgencyId,
    string    AgencyName,
    long?     CaregiverId,
    string?   CaregiverName,
    string    ServiceType,
    string    ShiftKind,
    DateOnly  Date,
    TimeOnly  StartTime,
    TimeOnly  EndTime,
    string    Status,
    decimal   Price,
    DateTime? AssignedAt,
    DateTime? CheckInAt,
    DateTime? CheckOutAt,
    DateTime? CompletedAt,
    string?   CancelReason,
    string?   CancelledBy,
    long?     ReplacesShiftId,
    string?   FamilyNote,
    DateTime  CreatedAt);

public sealed record ShiftListItem(
    long     Id,
    long     SeniorId,
    string   SeniorName,
    long?    CaregiverId,
    string?  CaregiverName,
    string   ShiftKind,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string   Status,
    decimal  Price);

public sealed record CoverageRiskItem(
    long     ShiftId,
    long     SeniorId,
    string   SeniorName,
    long     AgencyId,
    string   AgencyName,
    DateOnly Date,
    TimeOnly StartTime,
    double   HoursUntilStart);
