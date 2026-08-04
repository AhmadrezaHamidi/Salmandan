using Care.Domain.Agency.Args;
using Care.Domain.Agency.Entities;
using Care.Domain.Agency.Enums;
using Care.Domain.Agency.Exceptions;
using Care.Domain.Shift.Enums;

namespace Care.Domain.Agency.Aggregates;

/// <summary>
/// Aggregate Root مؤسسه پرستاری — طرف عرضه پلتفرم.
/// پلتفرم خودش اعزام‌کننده نیست؛ نیرو از مؤسسات دارای مجوز تأمین می‌شود.
/// </summary>
public sealed class CareAgency : AggregateRoot<long>
{
    private readonly List<AgencyServicePrice> _prices = [];

    public string       Name              { get; private set; } = string.Empty;
    public string       LicenseNumber     { get; private set; } = string.Empty;
    public DateOnly     LicenseExpiryDate { get; private set; }
    public long         OwnerUserId       { get; private set; }
    public string       City              { get; private set; } = string.Empty;
    public string?      District          { get; private set; }
    public string?      Address           { get; private set; }
    public string       PhoneNumber       { get; private set; } = string.Empty;
    public AgencyStatus Status            { get; private set; }

    /// <summary>درصد کمیسیون پلتفرم از هر شیفت</summary>
    public decimal      CommissionPercent { get; private set; }

    public decimal      RatingAvg         { get; private set; }
    public int          RatingCount       { get; private set; }
    public int          CompletedShifts   { get; private set; }
    public string?      RejectionReason   { get; private set; }
    public DateTime     CreatedAt         { get; private set; }

    public IReadOnlyCollection<AgencyServicePrice> Prices => _prices.AsReadOnly();

    /// <summary>مجوز معتبر است؟ — پایه‌ی کنترل «۹۰٪ مراکز غیرفعال»</summary>
    public bool HasValidLicense => LicenseExpiryDate >= DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>آیا مؤسسه می‌تواند شیفت بپذیرد</summary>
    public bool CanAcceptShifts => Status == AgencyStatus.Active && HasValidLicense;

    private CareAgency() { }

    private CareAgency(CreateAgencyArg arg) : base(arg.Id)
    {
        Name              = arg.Name;
        LicenseNumber     = arg.LicenseNumber;
        LicenseExpiryDate = arg.LicenseExpiryDate;
        OwnerUserId       = arg.OwnerUserId;
        City              = arg.City;
        District          = arg.District;
        Address           = arg.Address;
        PhoneNumber       = arg.PhoneNumber;
        CommissionPercent = arg.CommissionPercent;
        Status            = AgencyStatus.Pending;
        CreatedAt         = DateTime.UtcNow;
    }

    /// <summary>ثبت مؤسسه جدید — در وضعیت در انتظار تأیید</summary>
    public static CareAgency Register(CreateAgencyArg arg)
    {
        if (arg.CommissionPercent is < 0 or > 100)
            throw new InvalidCommissionException();

        var agency = new CareAgency(arg);
        agency.RaiseDomainEvent(new AgencyRegisteredEvent(arg.Id, arg.Name, arg.LicenseNumber, arg.OwnerUserId));
        return agency;
    }

    /// <summary>تأیید مؤسسه توسط ادمین پلتفرم پس از بررسی مجوز</summary>
    public void Approve()
    {
        if (Status == AgencyStatus.Active) throw new AgencyAlreadyApprovedException();
        if (!HasValidLicense)              throw new AgencyLicenseExpiredException();

        Status          = AgencyStatus.Active;
        RejectionReason = null;
        RaiseDomainEvent(new AgencyApprovedEvent(Id, Name));
    }

    /// <summary>رد درخواست مؤسسه</summary>
    public void Reject(string reason)
    {
        if (Status == AgencyStatus.Active) throw new AgencyAlreadyApprovedException();

        Status          = AgencyStatus.Rejected;
        RejectionReason = reason;
    }

    /// <summary>تعلیق مؤسسه — تخلف یا انقضای مجوز</summary>
    public void Suspend(string reason)
    {
        if (Status == AgencyStatus.Suspended) throw new AgencyAlreadySuspendedException();

        Status          = AgencyStatus.Suspended;
        RejectionReason = reason;
        RaiseDomainEvent(new AgencySuspendedEvent(Id, reason));
    }

    /// <summary>تمدید یا اصلاح مجوز وزارت بهداشت</summary>
    public void RenewLicense(string licenseNumber, DateOnly expiryDate)
    {
        if (expiryDate < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new AgencyLicenseExpiredException();

        LicenseNumber     = licenseNumber;
        LicenseExpiryDate = expiryDate;
    }

    /// <summary>بروزرسانی اطلاعات تماس و آدرس</summary>
    public void UpdateProfile(string name, string city, string? district, string? address, string phoneNumber)
    {
        Name        = name;
        City        = city;
        District    = district;
        Address     = address;
        PhoneNumber = phoneNumber;
    }

    /// <summary>تغییر درصد کمیسیون پلتفرم</summary>
    public void SetCommission(decimal percent)
    {
        if (percent is < 0 or > 100) throw new InvalidCommissionException();
        CommissionPercent = percent;
    }

    /// <summary>ثبت یا بروزرسانی تعرفه یک نوع خدمت — مبلغ خانواده و دستمزد پرستار</summary>
    public void SetServicePrice(long priceId, CareServiceType serviceType, ShiftKind shiftKind,
                                decimal price, decimal caregiverPayRate)
    {
        var existing = _prices.FirstOrDefault(p => p.ServiceType == serviceType && p.ShiftKind == shiftKind);
        if (existing is not null)
            existing.ChangeRates(price, caregiverPayRate);
        else
            _prices.Add(new AgencyServicePrice(priceId, Id, serviceType, shiftKind, price, caregiverPayRate));
    }

    /// <summary>تعرفه مصوب مؤسسه برای یک خدمت — مبلغ خانواده و دستمزد پرستار با هم</summary>
    public (decimal Price, decimal CaregiverPayRate) GetRates(CareServiceType serviceType, ShiftKind shiftKind)
    {
        var tariff = _prices.FirstOrDefault(p => p.ServiceType == serviceType && p.ShiftKind == shiftKind)
            ?? throw new ServicePriceNotFoundException();

        return (tariff.Price, tariff.CaregiverPayRate);
    }

    /// <summary>ثبت امتیاز جدید پس از تکمیل شیفت</summary>
    public void AddRating(int score)
    {
        if (score is < 1 or > 5) throw new InvalidRatingException();

        RatingAvg   = ((RatingAvg * RatingCount) + score) / (RatingCount + 1);
        RatingCount += 1;
    }

    /// <summary>افزایش شمارنده شیفت‌های تکمیل‌شده</summary>
    public void IncrementCompletedShifts() => CompletedShifts += 1;
}
