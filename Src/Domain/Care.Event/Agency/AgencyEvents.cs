using AhmadBase.Doamin;

namespace Care.Domain.Events.Agency;

/// <summary>رویداد ثبت مؤسسه پرستاری جدید (در انتظار تأیید)</summary>
public sealed record AgencyRegisteredEvent(long AgencyId, string Name, string LicenseNumber, long OwnerUserId) : IEvent;

/// <summary>رویداد تأیید مؤسسه توسط ادمین پلتفرم</summary>
public sealed record AgencyApprovedEvent(long AgencyId, string Name) : IEvent;

/// <summary>رویداد تعلیق مؤسسه</summary>
public sealed record AgencySuspendedEvent(long AgencyId, string Reason) : IEvent;

/// <summary>رویداد نزدیک شدن انقضای مجوز وزارت بهداشت</summary>
public sealed record AgencyLicenseExpiringEvent(long AgencyId, DateOnly ExpiryDate) : IEvent;
