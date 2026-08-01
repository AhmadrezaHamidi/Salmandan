using Care.Domain.Agency.Enums;
using Care.Domain.Shift.Enums;

namespace Care.Domain.Agency.Entities;

/// <summary>قیمت مصوب مؤسسه برای یک نوع خدمت در یک نوع شیفت</summary>
public sealed class AgencyServicePrice
{
    public long            Id          { get; private set; }
    public long            AgencyId    { get; private set; }
    public CareServiceType ServiceType { get; private set; }
    public ShiftKind       ShiftKind   { get; private set; }

    /// <summary>قیمت به ریال</summary>
    public decimal         Price       { get; private set; }
    public DateTime        UpdatedAt   { get; private set; }

    private AgencyServicePrice() { }

    public AgencyServicePrice(long id, long agencyId, CareServiceType serviceType, ShiftKind shiftKind, decimal price)
    {
        Id          = id;
        AgencyId    = agencyId;
        ServiceType = serviceType;
        ShiftKind   = shiftKind;
        Price       = price;
        UpdatedAt   = DateTime.UtcNow;
    }

    public void ChangePrice(decimal price)
    {
        Price     = price;
        UpdatedAt = DateTime.UtcNow;
    }
}
