using Care.Domain.Agency.Exceptions;
using Care.Domain.Caregiver.Exceptions;
using Care.Domain.Identity.Exceptions;
using Care.Domain.Senior.Exceptions;
using Care.Domain.Shift.Aggregates;
using Care.Domain.Shift.Args;
using Care.Domain.Shift.Exceptions;

namespace Care.Application.Shift;

/// <summary>
/// Handler های چرخه حیات شیفت مراقبت.
/// قیمت هرگز از ورودی کاربر گرفته نمی‌شود — از تعرفه مصوب مؤسسه خوانده می‌شود.
/// </summary>
public sealed class ShiftHandlers(
    IShiftRepository     shiftRepo,
    ISeniorRepository    seniorRepo,
    IAgencyRepository    agencyRepo,
    ICaregiverRepository caregiverRepo,
    CareDbContext        db) :
    ICommandHandler<RequestShiftCommand, long>,
    ICommandHandler<AssignCaregiverCommand, bool>,
    ICommandHandler<CheckInShiftCommand, bool>,
    ICommandHandler<CheckOutShiftCommand, bool>,
    ICommandHandler<CompleteShiftCommand, bool>,
    ICommandHandler<CancelShiftCommand, bool>,
    ICommandHandler<MarkShiftNoShowCommand, bool>,
    ICommandHandler<ReplaceShiftCommand, long>
{
    public async Task<long> Handle(RequestShiftCommand command, CancellationToken token)
    {
        var senior = await seniorRepo.GetWithFamilyAsync(command.SeniorId, token)
            ?? throw new SeniorNotFoundException();

        if (!senior.IsActive)                              throw new SeniorNotActiveException();
        if (!senior.CanBookShift(command.RequestedByUserId)) throw new AccessDeniedException();

        var agency = await agencyRepo.GetByIdAsync(command.AgencyId, token)
            ?? throw new AgencyNotFoundException();

        if (!agency.CanAcceptShifts) throw new AgencyNotActiveException();

        // هر دو نرخ از تعرفه مصوب مؤسسه — نه ورودی کاربر.
        // دستمزد پرستار همین‌جا snapshot می‌شود تا تغییر بعدی تعرفه حقوق این شیفت را عوض نکند.
        var (price, payRate) = agency.GetRates(command.ServiceType, command.ShiftKind);

        var id    = await shiftRepo.GetNextIdAsync();
        var shift = CareShift.Request(new CreateShiftArg(
            id, command.SeniorId, command.AgencyId, command.RequestedByUserId,
            command.ServiceType, command.ShiftKind, command.Date,
            command.StartTime, command.EndTime, price, payRate, command.FamilyNote));

        await shiftRepo.AddAsync(shift, token);
        await db.SaveChangesAsync(token);
        return id;
    }

    public async Task<bool> Handle(AssignCaregiverCommand command, CancellationToken token)
    {
        var shift = await GetShiftAsync(command.ShiftId, token);
        await AssignAsync(shift, command.CaregiverId, token);
        await shiftRepo.UpdateAsync(shift, token);
        await db.SaveChangesAsync(token);
        return true;
    }

    public async Task<bool> Handle(CheckInShiftCommand command, CancellationToken token)
    {
        var shift = await GetShiftAsync(command.ShiftId, token);

        var caregiver = await caregiverRepo.GetByUserIdAsync(command.ActorUserId, token)
            ?? throw new CaregiverNotFoundException();

        if (shift.CaregiverId != caregiver.Id) throw new AccessDeniedException();

        var senior = await seniorRepo.GetByIdAsync(shift.SeniorId, token)
            ?? throw new SeniorNotFoundException();

        shift.CheckIn(command.Latitude, command.Longitude, senior.Latitude, senior.Longitude);
        await shiftRepo.UpdateAsync(shift, token);
        await db.SaveChangesAsync(token);
        return true;
    }

    public async Task<bool> Handle(CheckOutShiftCommand command, CancellationToken token)
    {
        var shift = await GetShiftAsync(command.ShiftId, token);

        var caregiver = await caregiverRepo.GetByUserIdAsync(command.ActorUserId, token)
            ?? throw new CaregiverNotFoundException();

        if (shift.CaregiverId != caregiver.Id) throw new AccessDeniedException();

        shift.CheckOut();
        await shiftRepo.UpdateAsync(shift, token);
        await db.SaveChangesAsync(token);
        return true;
    }

    public async Task<bool> Handle(CompleteShiftCommand command, CancellationToken token)
    {
        var shift = await GetShiftAsync(command.ShiftId, token);
        shift.Complete();

        // بروزرسانی شمارنده‌های آماری مؤسسه و پرستار
        var agency = await agencyRepo.GetByIdAsync(shift.AgencyId, token);
        agency?.IncrementCompletedShifts();
        if (agency is not null) await agencyRepo.UpdateAsync(agency, token);

        if (shift.CaregiverId.HasValue)
        {
            var caregiver = await caregiverRepo.GetByIdAsync(shift.CaregiverId.Value, token);
            caregiver?.IncrementCompletedShifts();
            if (caregiver is not null) await caregiverRepo.UpdateAsync(caregiver, token);
        }

        await shiftRepo.UpdateAsync(shift, token);
        await db.SaveChangesAsync(token);
        return true;
    }

    public async Task<bool> Handle(CancelShiftCommand command, CancellationToken token)
    {
        var shift = await GetShiftAsync(command.ShiftId, token);
        shift.Cancel(command.CancelledBy, command.Reason);
        await shiftRepo.UpdateAsync(shift, token);
        await db.SaveChangesAsync(token);
        return true;
    }

    public async Task<bool> Handle(MarkShiftNoShowCommand command, CancellationToken token)
    {
        var shift = await GetShiftAsync(command.ShiftId, token);
        shift.MarkNoShow();

        // عدم حضور به پرونده پرستار ثبت می‌شود؛ سه بار = تعلیق خودکار
        if (shift.CaregiverId.HasValue)
        {
            var caregiver = await caregiverRepo.GetByIdAsync(shift.CaregiverId.Value, token);
            if (caregiver is not null)
            {
                caregiver.RegisterNoShow();
                await caregiverRepo.UpdateAsync(caregiver, token);
            }
        }

        await shiftRepo.UpdateAsync(shift, token);
        await db.SaveChangesAsync(token);
        return true;
    }

    /// <summary>
    /// ایجاد شیفت جایگزین برای شیفتی که پرستارش نیامده.
    /// اجرای وعده «تضمین پوشش شیفت» — شیفت جدید بدون هزینه اضافه برای خانواده ساخته می‌شود.
    /// </summary>
    public async Task<long> Handle(ReplaceShiftCommand command, CancellationToken token)
    {
        var original = await GetShiftAsync(command.ShiftId, token);

        var newId = await shiftRepo.GetNextIdAsync();
        var replacement = CareShift.Request(new CreateShiftArg(
            newId, original.SeniorId, original.AgencyId, original.RequestedByUserId,
            original.ServiceType, original.ShiftKind, original.Date,
            original.StartTime, original.EndTime, original.Price, original.CaregiverPayRate,
            original.FamilyNote, ReplacesShiftId: original.Id));

        await AssignAsync(replacement, command.ReplacementCaregiverId, token);
        await shiftRepo.AddAsync(replacement, token);

        original.MarkReplacedBy(newId);
        await shiftRepo.UpdateAsync(original, token);

        await db.SaveChangesAsync(token);
        return newId;
    }

    // ── کمکی ──────────────────────────────────────────────────────────────

    /// <summary>اعتبارسنجی و تخصیص پرستار — صلاحیت، تعلق به مؤسسه و عدم تداخل زمانی</summary>
    private async Task AssignAsync(CareShift shift, long caregiverId, CancellationToken ct)
    {
        var caregiver = await caregiverRepo.GetByIdAsync(caregiverId, ct)
            ?? throw new CaregiverNotFoundException();

        if (!caregiver.CanBeAssigned)          throw new CaregiverNotVerifiedException();
        if (caregiver.AgencyId != shift.AgencyId) throw new CaregiverNotInAgencyException();

        var overlaps = await shiftRepo.HasOverlappingShiftAsync(
            caregiverId, shift.Date, shift.StartTime, shift.EndTime, shift.Id, ct);

        if (overlaps) throw new CaregiverShiftOverlapException();

        shift.AssignCaregiver(caregiverId);
    }

    private async Task<CareShift> GetShiftAsync(long id, CancellationToken ct)
        => await shiftRepo.GetByIdAsync(id, ct) ?? throw new ShiftNotFoundException();
}
