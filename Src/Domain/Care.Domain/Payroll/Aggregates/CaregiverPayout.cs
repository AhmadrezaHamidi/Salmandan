using Care.Domain.Payroll.Args;
using Care.Domain.Payroll.Enums;
using Care.Domain.Payroll.Exceptions;

namespace Care.Domain.Payroll.Aggregates;

/// <summary>
/// Aggregate Root فیش حقوقی پرستار برای یک بازه.
///
/// مبلغ از دستمزد شیفت‌های تکمیل‌شده حساب می‌شود و شیفت‌ها به این فیش قفل می‌شوند
/// تا در دوره‌ی بعد دوباره پرداخت نشوند. ساعت کارکرد از چک‌این/چک‌اوت واقعی می‌آید،
/// نه از دفتر دستی — همین باعث می‌شود محاسبه از روش سنتی دقیق‌تر باشد.
/// </summary>
public sealed class CaregiverPayout : AggregateRoot<long>
{
    public long         AgencyId         { get; private set; }
    public long         CaregiverId      { get; private set; }
    public DateOnly     PeriodFrom       { get; private set; }
    public DateOnly     PeriodTo         { get; private set; }

    public int          ShiftCount       { get; private set; }
    public decimal      TotalHours       { get; private set; }

    /// <summary>مجموع دستمزد شیفت‌ها به ریال</summary>
    public decimal      TotalAmount      { get; private set; }

    public PayoutStatus Status           { get; private set; }
    public DateTime?    PaidAt           { get; private set; }

    /// <summary>شماره پیگیری واریز — برای پاسخگویی به پرستار</summary>
    public string?      PaymentReference { get; private set; }

    public string?      CancelReason     { get; private set; }
    public DateTime     CreatedAt        { get; private set; }

    /// <summary>میانگین دستمزد ساعتی این دوره</summary>
    public decimal HourlyRate => TotalHours > 0 ? Math.Round(TotalAmount / TotalHours) : 0;

    private CaregiverPayout() { }

    private CaregiverPayout(CreatePayoutArg arg) : base(arg.Id)
    {
        AgencyId    = arg.AgencyId;
        CaregiverId = arg.CaregiverId;
        PeriodFrom  = arg.PeriodFrom;
        PeriodTo    = arg.PeriodTo;
        ShiftCount  = arg.ShiftCount;
        TotalHours  = arg.TotalHours;
        TotalAmount = arg.TotalAmount;
        Status      = PayoutStatus.Pending;
        CreatedAt   = DateTime.UtcNow;
    }

    /// <summary>صدور فیش حقوقی برای یک پرستار در یک بازه</summary>
    public static CaregiverPayout Create(CreatePayoutArg arg)
    {
        if (arg.PeriodTo < arg.PeriodFrom) throw new InvalidPayoutPeriodException();
        if (arg.ShiftCount <= 0)           throw new NoShiftsToPayException();

        var payout = new CaregiverPayout(arg);
        payout.RaiseDomainEvent(new PayoutGeneratedEvent(
            arg.Id, arg.AgencyId, arg.CaregiverId, arg.ShiftCount, arg.TotalAmount));

        return payout;
    }

    /// <summary>ثبت پرداخت به پرستار</summary>
    public void MarkPaid(string paymentReference)
    {
        if (Status == PayoutStatus.Paid)      throw new PayoutAlreadyPaidException();
        if (Status == PayoutStatus.Cancelled) throw new PayoutCancelledException();
        if (string.IsNullOrWhiteSpace(paymentReference)) throw new PaymentReferenceRequiredException();

        Status           = PayoutStatus.Paid;
        PaidAt           = DateTime.UtcNow;
        PaymentReference = paymentReference;

        RaiseDomainEvent(new PayoutPaidEvent(Id, CaregiverId, TotalAmount, paymentReference));
    }

    /// <summary>لغو فیش — شیفت‌هایش باید در لایه Application آزاد شوند</summary>
    public void Cancel(string reason)
    {
        if (Status == PayoutStatus.Paid) throw new PayoutAlreadyPaidException();

        Status       = PayoutStatus.Cancelled;
        CancelReason = reason;
    }
}
