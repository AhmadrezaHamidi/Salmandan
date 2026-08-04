using Care.Domain.Payroll.Enums;

namespace Payroll.Application.Queries;

public record GetPayoutQuery(long Id) : IQuery<PayoutResult?>;

public record GetPayoutsQuery(
    int           Page = 1,
    int           PageSize = 20,
    long?         AgencyId = null,
    long?         CaregiverId = null,
    PayoutStatus? Status = null) : IQuery<PagedResult<PayoutListItem>>;

/// <summary>
/// پیش‌نمایش حقوق قبل از صدور فیش — مؤسسه ببیند چقدر باید بدهد و بابت چه شیفت‌هایی.
/// هیچ چیزی را قفل نمی‌کند.
/// </summary>
public record PreviewPayrollQuery(long AgencyId, DateOnly From, DateOnly To) : IQuery<IReadOnlyList<PayrollPreviewItem>>;

/// <summary>
/// سود و زیان مؤسسه در یک بازه — درآمد از خانواده منهای دستمزد پرستار.
/// عددی که صاحب مؤسسه هر ماه لازم دارد.
/// </summary>
public record GetAgencyProfitQuery(long AgencyId, DateOnly From, DateOnly To) : IQuery<AgencyProfitResult>;

public sealed record PayoutResult(
    long      Id,
    long      AgencyId,
    long      CaregiverId,
    string    CaregiverName,
    DateOnly  PeriodFrom,
    DateOnly  PeriodTo,
    int       ShiftCount,
    decimal   TotalHours,
    decimal   TotalAmount,
    decimal   HourlyRate,
    string    Status,
    DateTime? PaidAt,
    string?   PaymentReference,
    DateTime  CreatedAt,
    IReadOnlyList<PayoutShiftItem> Shifts);

public sealed record PayoutListItem(
    long     Id,
    long     CaregiverId,
    string   CaregiverName,
    DateOnly PeriodFrom,
    DateOnly PeriodTo,
    int      ShiftCount,
    decimal  TotalAmount,
    string   Status,
    DateTime CreatedAt);

/// <summary>ریز شیفت‌های یک فیش — پرستار باید بتواند بابت هر شیفت را ببیند</summary>
public sealed record PayoutShiftItem(
    long     ShiftId,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    decimal  WorkedHours,
    decimal  Amount);

public sealed record PayrollPreviewItem(
    long    CaregiverId,
    string  CaregiverName,
    int     ShiftCount,
    decimal TotalHours,
    decimal TotalAmount);

public sealed record AgencyProfitResult(
    long     AgencyId,
    DateOnly From,
    DateOnly To,
    int      CompletedShifts,
    decimal  Revenue,
    decimal  CaregiverCost,
    decimal  PlatformFee,
    decimal  GrossProfit,
    decimal  MarginPercent);
