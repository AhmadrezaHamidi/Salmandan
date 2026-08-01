using Care.Domain.Caregiver.Exceptions;
using Care.Domain.Identity.Exceptions;
using Care.Domain.Report.Aggregates;
using Care.Domain.Report.Args;
using Care.Domain.Report.Exceptions;
using Care.Domain.Senior.Exceptions;
using Care.Domain.Shift.Exceptions;

namespace Care.Application.Report;

/// <summary>
/// Handler های گزارش شیفت — لایه‌ای که با گذر زمان به موات محصول تبدیل می‌شود.
/// هر شیفت یک ردیف داده ساختاریافته می‌سازد؛ رقیب تازه‌وارد این تاریخچه را ندارد.
/// </summary>
public sealed class ReportHandlers(
    IShiftReportRepository reportRepo,
    IShiftRepository       shiftRepo,
    ISeniorRepository      seniorRepo,
    ICaregiverRepository   caregiverRepo,
    CareDbContext          db) :
    ICommandHandler<SubmitShiftReportCommand, long>,
    ICommandHandler<RecordMedicationIntakeCommand, bool>,
    ICommandHandler<FlagIncidentCommand, bool>,
    ICommandHandler<ApproveShiftReportCommand, bool>
{
    public async Task<long> Handle(SubmitShiftReportCommand command, CancellationToken token)
    {
        var shift = await shiftRepo.GetByIdAsync(command.ShiftId, token)
            ?? throw new ShiftNotFoundException();

        var caregiver = await caregiverRepo.GetByUserIdAsync(command.ActorUserId, token)
            ?? throw new CaregiverNotFoundException();

        if (shift.CaregiverId != caregiver.Id) throw new AccessDeniedException();

        if (await reportRepo.GetByShiftIdAsync(command.ShiftId, token) is not null)
            throw new ReportAlreadySubmittedException();

        var id     = await reportRepo.GetNextIdAsync();
        var report = ShiftReport.Submit(new SubmitReportArg(
            id, shift.Id, shift.SeniorId, caregiver.Id,
            command.Mood, command.Appetite, command.SleepQuality, command.Mobility, command.PainLevel,
            command.BloodPressureSystolic, command.BloodPressureDiastolic, command.PulseRate,
            command.Temperature, command.BloodSugar, command.OxygenSaturation, command.Notes));

        // داروهای فعال سالمند به‌صورت خودکار در چک‌لیست گزارش قرار می‌گیرند
        var senior = await seniorRepo.GetFullAsync(shift.SeniorId, token)
            ?? throw new SeniorNotFoundException();

        var intakeId = await reportRepo.GetNextIntakeIdAsync();
        foreach (var medication in senior.ActiveMedications)
        {
            report.RecordMedicationIntake(intakeId++, medication.Id, medication.Name, false, null);
        }

        await reportRepo.AddAsync(report, token);
        await db.SaveChangesAsync(token);
        return id;
    }

    public async Task<bool> Handle(RecordMedicationIntakeCommand command, CancellationToken token)
    {
        var report = await GetReportAsync(command.ReportId, token);

        var senior = await seniorRepo.GetFullAsync(report.SeniorId, token)
            ?? throw new SeniorNotFoundException();

        var medication = senior.Medications.FirstOrDefault(m => m.Id == command.MedicationId)
            ?? throw new MedicationNotFoundException();

        var intakeId = await reportRepo.GetNextIntakeIdAsync();
        report.SetMedicationIntake(intakeId, command.MedicationId, medication.Name, command.WasTaken, command.Reason);

        await reportRepo.UpdateAsync(report, token);
        await db.SaveChangesAsync(token);
        return true;
    }

    public async Task<bool> Handle(FlagIncidentCommand command, CancellationToken token)
    {
        var report = await GetReportAsync(command.ReportId, token);
        report.FlagIncident(command.IncidentKind, command.Description);
        await reportRepo.UpdateAsync(report, token);
        await db.SaveChangesAsync(token);
        return true;
    }

    public async Task<bool> Handle(ApproveShiftReportCommand command, CancellationToken token)
    {
        var report = await GetReportAsync(command.ReportId, token);

        var senior = await seniorRepo.GetWithFamilyAsync(report.SeniorId, token)
            ?? throw new SeniorNotFoundException();

        if (!senior.CanBookShift(command.ActorUserId)) throw new AccessDeniedException();

        report.ApproveByFamily();
        await reportRepo.UpdateAsync(report, token);
        await db.SaveChangesAsync(token);
        return true;
    }

    private async Task<ShiftReport> GetReportAsync(long id, CancellationToken ct)
        => await reportRepo.GetByIdAsync(id, ct) ?? throw new ReportNotFoundException();
}
