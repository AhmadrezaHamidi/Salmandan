using AhmadBase.Doamin;

namespace Care.Domain.Events.Caregiver;

/// <summary>رویداد ثبت پرستار جدید توسط مؤسسه</summary>
public sealed record CaregiverRegisteredEvent(long CaregiverId, long AgencyId, string FullName) : IEvent;

/// <summary>رویداد احراز صلاحیت پرستار (مدارک + سوءپیشینه)</summary>
public sealed record CaregiverVerifiedEvent(long CaregiverId, long AgencyId) : IEvent;

/// <summary>رویداد رد صلاحیت پرستار</summary>
public sealed record CaregiverRejectedEvent(long CaregiverId, string Reason) : IEvent;

/// <summary>رویداد تعلیق پرستار</summary>
public sealed record CaregiverSuspendedEvent(long CaregiverId, string Reason) : IEvent;
