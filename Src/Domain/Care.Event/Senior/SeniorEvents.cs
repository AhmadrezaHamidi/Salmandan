using AhmadBase.Doamin;

namespace Care.Domain.Events.Senior;

/// <summary>رویداد ایجاد پرونده مراقبت سالمند</summary>
public sealed record SeniorProfileCreatedEvent(long SeniorId, string FullName, long PrimaryFamilyUserId) : IEvent;

/// <summary>رویداد تغییر سطح وابستگی سالمند</summary>
public sealed record SeniorDependencyChangedEvent(long SeniorId, string OldLevel, string NewLevel) : IEvent;

/// <summary>رویداد افزودن عضو به حلقه خانواده</summary>
public sealed record FamilyMemberAddedEvent(long SeniorId, long UserId, string Relation, string AccessLevel) : IEvent;

/// <summary>رویداد حذف عضو از حلقه خانواده</summary>
public sealed record FamilyMemberRemovedEvent(long SeniorId, long UserId) : IEvent;

/// <summary>رویداد افزودن داروی جدید به پرونده</summary>
public sealed record MedicationAddedEvent(long SeniorId, string MedicationName, string Dosage) : IEvent;
