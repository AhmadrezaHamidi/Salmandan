using Care.Domain.Report.Enums;

namespace Care.Domain.Report.Args;

public sealed record SubmitReportArg(
    long             Id,
    long             ShiftId,
    long             SeniorId,
    long             CaregiverId,
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
    string?          Notes);
