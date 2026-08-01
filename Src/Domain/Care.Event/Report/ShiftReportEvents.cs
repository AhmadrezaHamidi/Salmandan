using AhmadBase.Doamin;

namespace Care.Domain.Events.Report;

/// <summary>رویداد ثبت گزارش پایان شیفت — خوراک لایه هوش بالینی</summary>
public sealed record ShiftReportSubmittedEvent(long ReportId, long ShiftId, long SeniorId, long CaregiverId) : IEvent;

/// <summary>رویداد ثبت حادثه در شیفت (سقوط، بستری، وخامت حال)</summary>
public sealed record IncidentReportedEvent(long ReportId, long SeniorId, string IncidentType, string Description) : IEvent;

/// <summary>رویداد تشخیص روند نزولی سلامت سالمند از روی گزارش‌های شیفت</summary>
public sealed record HealthDeteriorationDetectedEvent(long SeniorId, string RiskFactor, int RiskScore) : IEvent;
