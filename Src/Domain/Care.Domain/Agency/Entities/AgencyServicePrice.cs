using Care.Domain.Agency.Enums;
using Care.Domain.Agency.Exceptions;
using Care.Domain.Shift.Enums;

namespace Care.Domain.Agency.Entities;

/// <summary>
/// تعرفه مؤسسه برای یک نوع خدمت در یک نوع شیفت.
///
/// دو نرخ نگه می‌دارد چون کسب‌وکار مؤسسه اختلاف همین دو است:
/// سود = آنچه از خانواده می‌گیری − آنچه به پرستار می‌دهی
/// </summary>
public sealed class AgencyServicePrice
{
    public long            Id          { get; private set; }
    public long            AgencyId    { get; private set; }
    public CareServiceType ServiceType { get; private set; }
    public ShiftKind       ShiftKind   { get; private set; }

    /// <summary>مبلغ دریافتی از خانواده (ریال)</summary>
    public decimal         Price           { get; private set; }

    /// <summary>دستمزد پرستار برای این شیفت (ریال)</summary>
    public decimal         CaregiverPayRate { get; private set; }

    public DateTime        UpdatedAt   { get; private set; }

    /// <summary>سود ناخالص مؤسسه از هر شیفت</summary>
    public decimal Margin => Price - CaregiverPayRate;

    /// <summary>درصد حاشیه سود</summary>
    public decimal MarginPercent => Price > 0 ? Math.Round(Margin / Price * 100, 1) : 0;

    private AgencyServicePrice() { }

    public AgencyServicePrice(
        long id, long agencyId, CareServiceType serviceType, ShiftKind shiftKind,
        decimal price, decimal caregiverPayRate)
    {
        Validate(price, caregiverPayRate);

        Id               = id;
        AgencyId         = agencyId;
        ServiceType      = serviceType;
        ShiftKind        = shiftKind;
        Price            = price;
        CaregiverPayRate = caregiverPayRate;
        UpdatedAt        = DateTime.UtcNow;
    }

    public void ChangeRates(decimal price, decimal caregiverPayRate)
    {
        Validate(price, caregiverPayRate);

        Price            = price;
        CaregiverPayRate = caregiverPayRate;
        UpdatedAt        = DateTime.UtcNow;
    }

    /// <summary>
    /// دستمزد پرستار نمی‌تواند از مبلغ خانواده بیشتر یا مساوی باشد —
    /// جلوی ثبت تعرفه‌ی زیان‌ده را از همان لحظه می‌گیرد.
    /// </summary>
    private static void Validate(decimal price, decimal caregiverPayRate)
    {
        if (price <= 0)                  throw new InvalidServicePriceException();
        if (caregiverPayRate <= 0)       throw new InvalidPayRateException();
        if (caregiverPayRate >= price)   throw new PayRateExceedsPriceException();
    }
}
