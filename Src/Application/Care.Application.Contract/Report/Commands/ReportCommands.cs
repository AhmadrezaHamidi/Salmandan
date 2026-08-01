using Care.Domain.Report.Enums;

namespace Report.Application.Commands;

/// <summary>
/// ثبت گزارش پایان شیفت توسط پرستار.
/// ساختاریافتگی این داده، پایه‌ی لایه هوش بالینی و تمایز اصلی محصول است.
/// </summary>
public record SubmitShiftReportCommand(
    long             ShiftId,
    ObservationScale Mood,
    ObservationScale Appetite,
    ObservationScale SleepQuality,
    MobilityLevel    Mobility,
    int              PainLevel,
    int?             BloodPressureSystolic,
    int?             BloodPressureDiastolic,
    int?             PulseRate,
    decimal?         Temperature,
    int?             BloodSugar,
    int?             OxygenSaturation,
    string?          Notes,
    [property: JsonIgnore] long ActorUserId) : ICommand<long>;

/// <summary>ثبت وضعیت مصرف یک دارو در شیفت</summary>
public record RecordMedicationIntakeCommand(
    [property: JsonIgnore] long ReportId,
    long    MedicationId,
    bool    WasTaken,
    string? Reason) : ICommand<bool>;

/// <summary>ثبت حادثه در شیفت — اطلاع فوری به خانواده</summary>
public record FlagIncidentCommand(
    [property: JsonIgnore] long ReportId,
    IncidentType IncidentKind,
    string       Description) : ICommand<bool>;

/// <summary>تأیید گزارش توسط خانواده — پیش‌شرط آزادسازی وجه</summary>
public record ApproveShiftReportCommand(
    [property: JsonIgnore] long ReportId,
    [property: JsonIgnore] long ActorUserId) : ICommand<bool>;
