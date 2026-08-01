using Care.Application.Contract.Services;

namespace Report.Application.Queries;

public record GetShiftReportQuery(long Id) : IQuery<ShiftReportResult?>;

public record GetReportByShiftQuery(long ShiftId) : IQuery<ShiftReportResult?>;

public record GetReportsBySeniorQuery(long SeniorId, int Page = 1, int PageSize = 20) : IQuery<PagedResult<ShiftReportListItem>>;

/// <summary>
/// تایم‌لاین سلامت سالمند — روند شاخص‌ها در N گزارش اخیر.
/// خوراک نمودارهای اپ خانواده.
/// </summary>
public record GetHealthTimelineQuery(long SeniorId, int Count = 14) : IQuery<IReadOnlyList<HealthTimelinePoint>>;

/// <summary>
/// تحلیل ریسک سلامت — «چطور بود این هفته؟» به زبان فارسی.
/// تمایز اصلی محصول نسبت به رقبای داخلی.
/// </summary>
public record GetHealthRiskQuery(long SeniorId) : IQuery<HealthRiskResult>;

public sealed record ShiftReportResult(
    long     Id,
    long     ShiftId,
    long     SeniorId,
    string   SeniorName,
    long     CaregiverId,
    string   CaregiverName,
    DateOnly ShiftDate,
    string   Mood,
    string   Appetite,
    string   SleepQuality,
    string   Mobility,
    int      PainLevel,
    int?     BloodPressureSystolic,
    int?     BloodPressureDiastolic,
    int?     PulseRate,
    decimal? Temperature,
    int?     BloodSugar,
    int?     OxygenSaturation,
    bool     HasIncident,
    string?  IncidentKind,
    string?  IncidentDescription,
    string?  Notes,
    bool     IsApprovedByFamily,
    DateTime SubmittedAt,
    IReadOnlyList<MedicationIntakeItem> MedicationIntakes);

public sealed record ShiftReportListItem(
    long     Id,
    long     ShiftId,
    DateOnly ShiftDate,
    string   CaregiverName,
    string   Mood,
    string   Appetite,
    int      PainLevel,
    bool     HasIncident,
    bool     IsApprovedByFamily,
    DateTime SubmittedAt);

public sealed record MedicationIntakeItem(long MedicationId, string MedicationName, bool WasTaken, string? Reason);

/// <summary>یک نقطه روی نمودار روند سلامت</summary>
public sealed record HealthTimelinePoint(
    DateTime SubmittedAt,
    DateOnly ShiftDate,
    int      MoodScore,
    int      AppetiteScore,
    int      SleepScore,
    int      MobilityScore,
    int      PainLevel,
    int?     BloodPressureSystolic,
    int?     PulseRate,
    bool     HasIncident);
