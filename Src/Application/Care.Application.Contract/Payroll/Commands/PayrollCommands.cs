namespace Payroll.Application.Commands;

/// <summary>
/// محاسبه حقوق پرستاران یک مؤسسه برای یک بازه.
/// همه‌ی شیفت‌های تکمیل‌شده‌ی پرداخت‌نشده در بازه برداشته می‌شود و برای هر پرستار
/// یک فیش صادر می‌گردد. شیفت‌ها به فیش قفل می‌شوند تا دوباره پرداخت نشوند.
/// </summary>
public record GeneratePayrollCommand(
    long     AgencyId,
    DateOnly From,
    DateOnly To) : ICommand<GeneratePayrollResponse>;

/// <summary>ثبت واریز حقوق به پرستار</summary>
public record MarkPayoutPaidCommand(
    [property: JsonIgnore] long PayoutId,
    string PaymentReference) : ICommand<bool>;

/// <summary>لغو فیش حقوقی — شیفت‌هایش دوباره قابل محاسبه می‌شوند</summary>
public record CancelPayoutCommand(
    [property: JsonIgnore] long PayoutId,
    string Reason) : ICommand<bool>;

public sealed record GeneratePayrollResponse(
    int     PayoutCount,
    int     ShiftCount,
    decimal TotalAmount,
    IReadOnlyList<long> PayoutIds);
