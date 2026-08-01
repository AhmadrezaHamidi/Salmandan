namespace Care.Domain.Report.Entities;

/// <summary>ثبت مصرف یا عدم مصرف یک دارو در یک شیفت</summary>
public sealed class MedicationIntake
{
    public long    Id             { get; private set; }
    public long    ReportId       { get; private set; }
    public long    MedicationId   { get; private set; }
    public string  MedicationName { get; private set; } = string.Empty;
    public bool    WasTaken       { get; private set; }

    /// <summary>دلیل عدم مصرف — اگر دارو خورده نشده</summary>
    public string? Reason         { get; private set; }

    private MedicationIntake() { }

    public MedicationIntake(long id, long reportId, long medicationId, string medicationName, bool wasTaken, string? reason)
    {
        Id             = id;
        ReportId       = reportId;
        MedicationId   = medicationId;
        MedicationName = medicationName;
        WasTaken       = wasTaken;
        Reason         = reason;
    }

    /// <summary>بروزرسانی وضعیت مصرف</summary>
    public void Update(bool wasTaken, string? reason)
    {
        WasTaken = wasTaken;
        Reason   = reason;
    }
}
