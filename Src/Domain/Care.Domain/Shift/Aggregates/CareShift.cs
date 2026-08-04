using Care.Domain.Agency.Enums;
using Care.Domain.Shift.Args;
using Care.Domain.Shift.Enums;
using Care.Domain.Shift.Exceptions;

namespace Care.Domain.Shift.Aggregates;

/// <summary>
/// Aggregate Root شیفت مراقبت — واحد عملیاتی و مالی پلتفرم.
/// چک‌این/چک‌اوت ژئوفنس تضمین می‌کند پرستار واقعاً در محل بوده است.
/// </summary>
public sealed class CareShift : AggregateRoot<long>
{
    /// <summary>حداکثر فاصله مجاز چک‌این از منزل سالمند (متر)</summary>
    public const double GeofenceRadiusMeters = 200;

    public long            SeniorId          { get; private set; }
    public long            AgencyId          { get; private set; }
    public long?           CaregiverId       { get; private set; }
    public long            RequestedByUserId { get; private set; }

    public CareServiceType ServiceType       { get; private set; }
    public ShiftKind       ShiftKind         { get; private set; }
    public DateOnly        Date              { get; private set; }
    public TimeOnly        StartTime         { get; private set; }
    public TimeOnly        EndTime           { get; private set; }
    public ShiftStatus     Status            { get; private set; }

    /// <summary>مبلغ دریافتی از خانواده به ریال</summary>
    public decimal         Price             { get; private set; }

    /// <summary>
    /// دستمزد پرستار برای این شیفت — از تعرفه‌ی لحظه‌ی ثبت کپی می‌شود.
    /// snapshot است تا تغییر بعدی تعرفه، حقوق شیفت‌های گذشته را عوض نکند.
    /// </summary>
    public decimal         CaregiverPayRate  { get; private set; }

    /// <summary>شناسه فیش حقوقی که این شیفت در آن حساب شده — جلوی پرداخت دوباره را می‌گیرد</summary>
    public long?           PayoutId          { get; private set; }

    public DateTime?       AssignedAt        { get; private set; }
    public DateTime?       CheckInAt         { get; private set; }
    public double?         CheckInLatitude   { get; private set; }
    public double?         CheckInLongitude  { get; private set; }
    public DateTime?       CheckOutAt        { get; private set; }
    public DateTime?       CompletedAt       { get; private set; }

    public string?         CancelReason      { get; private set; }
    public CancelActor?    CancelledBy       { get; private set; }

    /// <summary>اگر این شیفت جایگزین شیفت دیگری است</summary>
    public long?           ReplacesShiftId   { get; private set; }

    public string?         FamilyNote        { get; private set; }
    public DateTime        CreatedAt         { get; private set; }

    /// <summary>مدت شیفت به ساعت</summary>
    public double DurationHours
    {
        get
        {
            var span = EndTime - StartTime;
            if (span < TimeSpan.Zero) span += TimeSpan.FromDays(1); // شیفت شب که از نیمه‌شب عبور می‌کند
            return span.TotalHours;
        }
    }

    /// <summary>زمان شروع شیفت به‌صورت DateTime</summary>
    public DateTime StartsAt => Date.ToDateTime(StartTime);

    /// <summary>سود ناخالص مؤسسه از این شیفت</summary>
    public decimal Margin => Price - CaregiverPayRate;

    /// <summary>
    /// ساعت کارکرد واقعی از چک‌این تا چک‌اوت.
    /// اگر ثبت نشده باشد به مدت برنامه‌ریزی‌شده برمی‌گردد.
    /// همین عدد است که محاسبه حقوق را از دفتر کاغذی قابل‌دفاع‌تر می‌کند.
    /// </summary>
    public double WorkedHours => CheckInAt.HasValue && CheckOutAt.HasValue
        ? (CheckOutAt.Value - CheckInAt.Value).TotalHours
        : DurationHours;

    /// <summary>آیا این شیفت در فیش حقوقی حساب شده است</summary>
    public bool IsPaidOut => PayoutId.HasValue;

    /// <summary>آیا شیفت هنوز قابل لغو است</summary>
    public bool IsCancellable => Status is ShiftStatus.Requested or ShiftStatus.Assigned;

    private CareShift() { }

    private CareShift(CreateShiftArg arg) : base(arg.Id)
    {
        SeniorId          = arg.SeniorId;
        AgencyId          = arg.AgencyId;
        RequestedByUserId = arg.RequestedByUserId;
        ServiceType       = arg.ServiceType;
        ShiftKind         = arg.ShiftKind;
        Date              = arg.Date;
        StartTime         = arg.StartTime;
        EndTime           = arg.EndTime;
        Price             = arg.Price;
        CaregiverPayRate  = arg.CaregiverPayRate;
        FamilyNote        = arg.FamilyNote;
        ReplacesShiftId   = arg.ReplacesShiftId;
        Status            = ShiftStatus.Requested;
        CreatedAt         = DateTime.UtcNow;
    }

    /// <summary>ثبت درخواست شیفت توسط خانواده</summary>
    public static CareShift Request(CreateShiftArg arg)
    {
        if (arg.Price <= 0)
            throw new InvalidShiftPriceException();

        if (arg.Date < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ShiftDateInPastException();

        var shift = new CareShift(arg);
        shift.RaiseDomainEvent(new ShiftRequestedEvent(arg.Id, arg.SeniorId, arg.AgencyId, arg.Date, arg.StartTime));
        return shift;
    }

    /// <summary>تخصیص پرستار توسط مؤسسه</summary>
    public void AssignCaregiver(long caregiverId)
    {
        if (Status is ShiftStatus.Cancelled)  throw new ShiftAlreadyCancelledException();
        if (Status is ShiftStatus.Completed)  throw new ShiftAlreadyCompletedException();
        if (Status is not (ShiftStatus.Requested or ShiftStatus.Assigned or ShiftStatus.NoShow))
            throw new InvalidShiftTransitionException();

        CaregiverId = caregiverId;
        AssignedAt  = DateTime.UtcNow;
        Status      = ShiftStatus.Assigned;
        RaiseDomainEvent(new ShiftAssignedEvent(Id, SeniorId, caregiverId));
    }

    /// <summary>
    /// چک‌این پرستار در محل.
    /// فاصله تا منزل سالمند باید کمتر از شعاع ژئوفنس باشد.
    /// </summary>
    public void CheckIn(double latitude, double longitude, double? seniorLatitude, double? seniorLongitude)
    {
        if (Status != ShiftStatus.Assigned) throw new InvalidShiftTransitionException();
        if (CaregiverId is null)            throw new ShiftNotAssignedException();

        if (seniorLatitude.HasValue && seniorLongitude.HasValue)
        {
            var distance = DistanceInMeters(latitude, longitude, seniorLatitude.Value, seniorLongitude.Value);
            if (distance > GeofenceRadiusMeters)
                throw new CheckInOutsideGeofenceException();
        }

        CheckInAt        = DateTime.UtcNow;
        CheckInLatitude  = latitude;
        CheckInLongitude = longitude;
        Status           = ShiftStatus.CheckedIn;
        RaiseDomainEvent(new ShiftCheckedInEvent(Id, SeniorId, CaregiverId.Value, CheckInAt.Value));
    }

    /// <summary>چک‌اوت پرستار — پس از آن باید گزارش شیفت ثبت شود</summary>
    public void CheckOut()
    {
        if (Status != ShiftStatus.CheckedIn) throw new InvalidShiftTransitionException();

        CheckOutAt = DateTime.UtcNow;
        Status     = ShiftStatus.CheckedOut;
        RaiseDomainEvent(new ShiftCheckedOutEvent(Id, SeniorId, CaregiverId!.Value, CheckOutAt.Value));
    }

    /// <summary>تکمیل شیفت پس از تأیید گزارش — تریگر آزادسازی وجه از امانت</summary>
    public void Complete()
    {
        if (Status == ShiftStatus.Completed) throw new ShiftAlreadyCompletedException();
        if (Status != ShiftStatus.CheckedOut) throw new ShiftReportRequiredException();

        CompletedAt = DateTime.UtcNow;
        Status      = ShiftStatus.Completed;
        RaiseDomainEvent(new ShiftCompletedEvent(Id, SeniorId, CaregiverId!.Value, AgencyId));
    }

    /// <summary>لغو شیفت</summary>
    public void Cancel(CancelActor cancelledBy, string? reason)
    {
        if (Status == ShiftStatus.Cancelled) throw new ShiftAlreadyCancelledException();
        if (Status == ShiftStatus.Completed) throw new ShiftAlreadyCompletedException();
        if (!IsCancellable)                  throw new ShiftInProgressException();

        Status       = ShiftStatus.Cancelled;
        CancelReason = reason;
        CancelledBy  = cancelledBy;
        RaiseDomainEvent(new ShiftCancelledEvent(Id, SeniorId, reason));
    }

    /// <summary>
    /// ثبت عدم حضور پرستار — تریگر تضمین پوشش شیفت (Coverage SLA).
    /// مؤسسه موظف است ظرف مهلت مقرر جایگزین بفرستد.
    /// </summary>
    public void MarkNoShow()
    {
        if (Status is not (ShiftStatus.Assigned or ShiftStatus.Requested))
            throw new InvalidShiftTransitionException();

        Status = ShiftStatus.NoShow;
        RaiseDomainEvent(new ShiftNoShowEvent(Id, SeniorId, AgencyId, CaregiverId));
    }

    /// <summary>ثبت اینکه این شیفت با شیفت جایگزین پوشش داده شد</summary>
    public void MarkReplacedBy(long replacementShiftId)
    {
        RaiseDomainEvent(new ShiftReplacedEvent(Id, replacementShiftId, SeniorId));
    }

    /// <summary>
    /// ثبت اینکه دستمزد این شیفت در یک فیش حقوقی حساب شده است.
    /// فقط شیفت تکمیل‌شده و فقط یک بار.
    /// </summary>
    public void IncludeInPayout(long payoutId)
    {
        if (Status != ShiftStatus.Completed) throw new ShiftNotCompletedException();
        if (PayoutId.HasValue)               throw new ShiftAlreadyPaidOutException();

        PayoutId = payoutId;
    }

    /// <summary>آزادسازی شیفت از فیش لغوشده تا در دوره بعد دوباره محاسبه شود</summary>
    public void ReleaseFromPayout() => PayoutId = null;

    /// <summary>فرمول هاورساین — فاصله دو نقطه روی کره زمین به متر</summary>
    private static double DistanceInMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadius = 6371000;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        return earthRadius * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}
