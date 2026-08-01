using Care.Domain.Report.Args;
using Care.Domain.Report.Entities;
using Care.Domain.Report.Enums;
using Care.Domain.Report.Exceptions;

namespace Care.Domain.Report.Aggregates;

/// <summary>
/// Aggregate Root گزارش پایان شیفت.
/// این ساختاریافتگی، خوراک لایه هوش بالینی است — بدون آن پیش‌بینی روند نزولی ممکن نیست.
/// </summary>
public sealed class ShiftReport : AggregateRoot<long>
{
    private readonly List<MedicationIntake> _medicationIntakes = [];

    public long             ShiftId          { get; private set; }
    public long             SeniorId         { get; private set; }
    public long             CaregiverId      { get; private set; }

    // ── شاخص‌های رفتاری ────────────────────────────────────────────────────
    public ObservationScale Mood             { get; private set; }
    public ObservationScale Appetite         { get; private set; }
    public ObservationScale SleepQuality     { get; private set; }
    public MobilityLevel    Mobility         { get; private set; }

    /// <summary>شدت درد ۰ تا ۱۰</summary>
    public int              PainLevel        { get; private set; }

    // ── علائم حیاتی (اختیاری) ──────────────────────────────────────────────
    public int?             BloodPressureSystolic  { get; private set; }
    public int?             BloodPressureDiastolic { get; private set; }
    public int?             PulseRate              { get; private set; }
    public decimal?         Temperature            { get; private set; }
    public int?             BloodSugar             { get; private set; }
    public int?             OxygenSaturation       { get; private set; }

    // ── حادثه ──────────────────────────────────────────────────────────────
    public bool             HasIncident         { get; private set; }
    public IncidentType?    IncidentKind        { get; private set; }
    public string?          IncidentDescription { get; private set; }

    public string?          Notes            { get; private set; }
    public DateTime         SubmittedAt      { get; private set; }

    /// <summary>گزارش توسط خانواده تأیید شده — پیش‌شرط آزادسازی وجه</summary>
    public bool             IsApprovedByFamily { get; private set; }
    public DateTime?        ApprovedAt         { get; private set; }

    public IReadOnlyCollection<MedicationIntake> MedicationIntakes => _medicationIntakes.AsReadOnly();

    /// <summary>نسبت داروهای مصرف‌شده به کل داروهای تجویزی این شیفت</summary>
    public decimal MedicationAdherence
        => _medicationIntakes.Count == 0
            ? 1m
            : (decimal)_medicationIntakes.Count(m => m.WasTaken) / _medicationIntakes.Count;

    /// <summary>
    /// امتیاز ریسک ۰ تا ۱۰۰ بر مبنای شاخص‌های این شیفت.
    /// ورودی خام لایه هوش؛ روند چند شیفت متوالی است که هشدار می‌سازد.
    /// </summary>
    public int RiskScore
    {
        get
        {
            var score = 0;

            if (Mood == ObservationScale.Poor)         score += 10;
            if (Appetite == ObservationScale.Poor)     score += 15;
            if (SleepQuality == ObservationScale.Poor) score += 10;
            if (Mobility == MobilityLevel.Bedridden)   score += 15;
            if (PainLevel >= 7)                        score += 15;
            if (MedicationAdherence < 0.8m)            score += 15;
            if (HasIncident)                           score += 20;

            // فشار خون خارج از محدوده نرمال
            if (BloodPressureSystolic is > 160 or < 90) score += 10;
            // نبض خارج از محدوده نرمال
            if (PulseRate is > 100 or < 50)             score += 10;
            // تب
            if (Temperature is > 38m)                   score += 10;
            // اشباع اکسیژن پایین
            if (OxygenSaturation is < 92)               score += 15;

            return Math.Min(score, 100);
        }
    }

    private ShiftReport() { }

    private ShiftReport(SubmitReportArg arg) : base(arg.Id)
    {
        ShiftId                = arg.ShiftId;
        SeniorId               = arg.SeniorId;
        CaregiverId            = arg.CaregiverId;
        Mood                   = arg.Mood;
        Appetite               = arg.Appetite;
        SleepQuality           = arg.SleepQuality;
        Mobility               = arg.Mobility;
        PainLevel              = arg.PainLevel;
        BloodPressureSystolic  = arg.BloodPressureSystolic;
        BloodPressureDiastolic = arg.BloodPressureDiastolic;
        PulseRate              = arg.PulseRate;
        Temperature            = arg.Temperature;
        BloodSugar             = arg.BloodSugar;
        OxygenSaturation       = arg.OxygenSaturation;
        Notes                  = arg.Notes;
        SubmittedAt            = DateTime.UtcNow;
    }

    /// <summary>ثبت گزارش پایان شیفت توسط پرستار</summary>
    public static ShiftReport Submit(SubmitReportArg arg)
    {
        if (arg.PainLevel is < 0 or > 10)
            throw new InvalidPainLevelException();

        if (arg.Temperature is < 30m or > 45m)
            throw new InvalidVitalSignException();

        var report = new ShiftReport(arg);
        report.RaiseDomainEvent(new ShiftReportSubmittedEvent(arg.Id, arg.ShiftId, arg.SeniorId, arg.CaregiverId));
        return report;
    }

    /// <summary>افزودن ردیف دارو به چک‌لیست گزارش</summary>
    public void RecordMedicationIntake(long intakeId, long medicationId, string medicationName, bool wasTaken, string? reason)
    {
        if (_medicationIntakes.Any(m => m.MedicationId == medicationId))
            throw new DuplicateMedicationIntakeException();

        _medicationIntakes.Add(new MedicationIntake(intakeId, Id, medicationId, medicationName, wasTaken, reason));
    }

    /// <summary>ثبت یا بروزرسانی وضعیت مصرف یک دارو</summary>
    public void SetMedicationIntake(long intakeId, long medicationId, string medicationName, bool wasTaken, string? reason)
    {
        var existing = _medicationIntakes.FirstOrDefault(m => m.MedicationId == medicationId);
        if (existing is not null)
            existing.Update(wasTaken, reason);
        else
            _medicationIntakes.Add(new MedicationIntake(intakeId, Id, medicationId, medicationName, wasTaken, reason));
    }

    /// <summary>ثبت حادثه — بلافاصله به خانواده اطلاع داده می‌شود</summary>
    public void FlagIncident(IncidentType incidentKind, string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new IncidentDescriptionRequiredException();

        HasIncident         = true;
        IncidentKind        = incidentKind;
        IncidentDescription = description;

        RaiseDomainEvent(new IncidentReportedEvent(Id, SeniorId, incidentKind.ToString(), description));
    }

    /// <summary>تأیید گزارش توسط خانواده — پیش‌شرط آزادسازی وجه از امانت</summary>
    public void ApproveByFamily()
    {
        if (IsApprovedByFamily) throw new ReportAlreadyApprovedException();

        IsApprovedByFamily = true;
        ApprovedAt         = DateTime.UtcNow;
    }

    /// <summary>اعلام تشخیص روند نزولی سلامت پس از تحلیل چند شیفت</summary>
    public void RaiseDeteriorationAlert(string riskFactor)
        => RaiseDomainEvent(new HealthDeteriorationDetectedEvent(SeniorId, riskFactor, RiskScore));
}
