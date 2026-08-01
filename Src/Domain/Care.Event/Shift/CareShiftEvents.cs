using AhmadBase.Doamin;

namespace Care.Domain.Events.Shift;

/// <summary>رویداد ثبت درخواست شیفت مراقبت توسط خانواده</summary>
public sealed record ShiftRequestedEvent(long ShiftId, long SeniorId, long AgencyId, DateOnly Date, TimeOnly StartTime) : IEvent;

/// <summary>رویداد تخصیص پرستار به شیفت توسط مؤسسه</summary>
public sealed record ShiftAssignedEvent(long ShiftId, long SeniorId, long CaregiverId) : IEvent;

/// <summary>رویداد ورود پرستار به محل (چک‌این ژئوفنس)</summary>
public sealed record ShiftCheckedInEvent(long ShiftId, long CaregiverId, DateTime CheckInAt) : IEvent;

/// <summary>رویداد خروج پرستار از محل</summary>
public sealed record ShiftCheckedOutEvent(long ShiftId, long CaregiverId, DateTime CheckOutAt) : IEvent;

/// <summary>رویداد تکمیل شیفت — تریگر آزادسازی وجه از امانت</summary>
public sealed record ShiftCompletedEvent(long ShiftId, long SeniorId, long CaregiverId, long AgencyId) : IEvent;

/// <summary>رویداد لغو شیفت</summary>
public sealed record ShiftCancelledEvent(long ShiftId, long SeniorId, string? Reason) : IEvent;

/// <summary>رویداد عدم حضور پرستار — تریگر تضمین پوشش شیفت (Coverage SLA)</summary>
public sealed record ShiftNoShowEvent(long ShiftId, long SeniorId, long AgencyId, long? CaregiverId) : IEvent;

/// <summary>رویداد جایگزینی شیفت با پرستار دیگر</summary>
public sealed record ShiftReplacedEvent(long OriginalShiftId, long ReplacementShiftId, long SeniorId) : IEvent;
