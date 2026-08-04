using AhmadBase.Persistence.NHiLoHelper;
using Care.Domain.Agency.Aggregates;
using Care.Domain.Agency.Entities;
using Care.Domain.Agency.Enums;
using Care.Domain.Agency.Repositories;
using Care.Domain.Billing.Aggregates;
using Care.Domain.Billing.Repositories;
using Care.Domain.Caregiver.Aggregates;
using Care.Domain.Caregiver.Entities;
using Care.Domain.Caregiver.Enums;
using Care.Domain.Caregiver.Repositories;
using Care.Domain.Payroll.Aggregates;
using Care.Domain.Payroll.Repositories;
using Care.Domain.Report.Aggregates;
using Care.Domain.Report.Entities;
using Care.Domain.Report.Repositories;
using Care.Domain.Senior.Aggregates;
using Care.Domain.Senior.Entities;
using Care.Domain.Senior.Repositories;
using Care.Domain.Shift.Aggregates;
using Care.Domain.Shift.Enums;
using Care.Domain.Shift.Repositories;

namespace Care.Persistence.EF.Repositories;

public sealed class AgencyRepository(CareDbContext db, IHiLoIdGenerator hiLo) : IAgencyRepository
{
    public Task<CareAgency?> GetByIdAsync(long id, CancellationToken ct = default)
        => db.Agencies.Include(a => a.Prices).FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<CareAgency?> GetByOwnerUserIdAsync(long ownerUserId, CancellationToken ct = default)
        => db.Agencies.Include(a => a.Prices).FirstOrDefaultAsync(a => a.OwnerUserId == ownerUserId, ct);

    public Task<bool> LicenseNumberExistsAsync(string licenseNumber, CancellationToken ct = default)
        => db.Agencies.AnyAsync(a => a.LicenseNumber == licenseNumber, ct);

    public Task<List<CareAgency>> GetActiveByCityAsync(string city, CancellationToken ct = default)
        => db.Agencies
            .Include(a => a.Prices)
            .Where(a => a.City == city && a.Status == AgencyStatus.Active)
            .OrderByDescending(a => a.RatingAvg)
            .ToListAsync(ct);

    public async Task AddAsync(CareAgency agency, CancellationToken ct = default)
        => await db.Agencies.AddAsync(agency, ct);

    public Task UpdateAsync(CareAgency agency, CancellationToken ct = default)
    {
        db.Agencies.Update(agency);
        return Task.CompletedTask;
    }

    public Task<long> GetNextIdAsync() => Task.FromResult(hiLo.GetNextId<CareAgency>());

    // HiLo فقط برای Aggregate Root است؛ برای موجودیت‌های فرزند از max+1 استفاده می‌شود
    public async Task<long> GetNextPriceIdAsync()
        => (await db.AgencyServicePrices.MaxAsync(p => (long?)p.Id) ?? 0) + 1;
}

public sealed class CaregiverRepository(CareDbContext db, IHiLoIdGenerator hiLo) : ICaregiverRepository
{
    public Task<CaregiverProfile?> GetByIdAsync(long id, CancellationToken ct = default)
        => db.Caregivers.Include(c => c.Skills).FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<CaregiverProfile?> GetByUserIdAsync(long userId, CancellationToken ct = default)
        => db.Caregivers.Include(c => c.Skills).FirstOrDefaultAsync(c => c.UserId == userId, ct);

    public Task<bool> NationalIdExistsAsync(string nationalId, CancellationToken ct = default)
        => db.Caregivers.AnyAsync(c => c.NationalId == nationalId, ct);

    public Task<List<CaregiverProfile>> GetVerifiedByAgencyAsync(long agencyId, CancellationToken ct = default)
        => db.Caregivers
            .Include(c => c.Skills)
            .Where(c => c.AgencyId == agencyId && c.Status == CaregiverStatus.Verified)
            .OrderByDescending(c => c.RatingAvg)
            .ToListAsync(ct);

    /// <summary>پرستاران تأییدشده‌ای که در بازه خواسته‌شده شیفت هم‌پوشان ندارند</summary>
    public async Task<List<CaregiverProfile>> GetAvailableAsync(
        long agencyId, DateOnly date, TimeOnly startTime, TimeOnly endTime, CancellationToken ct = default)
    {
        var busyIds = await db.Shifts
            .Where(s => s.AgencyId == agencyId
                     && s.Date == date
                     && s.CaregiverId != null
                     && s.Status != ShiftStatus.Cancelled
                     && s.StartTime < endTime
                     && startTime < s.EndTime)
            .Select(s => s.CaregiverId!.Value)
            .Distinct()
            .ToListAsync(ct);

        return await db.Caregivers
            .Include(c => c.Skills)
            .Where(c => c.AgencyId == agencyId
                     && c.Status == CaregiverStatus.Verified
                     && !busyIds.Contains(c.Id))
            .OrderByDescending(c => c.RatingAvg)
            .ToListAsync(ct);
    }

    public async Task AddAsync(CaregiverProfile caregiver, CancellationToken ct = default)
        => await db.Caregivers.AddAsync(caregiver, ct);

    public Task UpdateAsync(CaregiverProfile caregiver, CancellationToken ct = default)
    {
        db.Caregivers.Update(caregiver);
        return Task.CompletedTask;
    }

    public Task<long> GetNextIdAsync() => Task.FromResult(hiLo.GetNextId<CaregiverProfile>());

    public async Task<long> GetNextSkillIdAsync()
        => (await db.CaregiverSkills.MaxAsync(s => (long?)s.Id) ?? 0) + 1;
}

public sealed class SeniorRepository(CareDbContext db, IHiLoIdGenerator hiLo) : ISeniorRepository
{
    public Task<SeniorProfile?> GetByIdAsync(long id, CancellationToken ct = default)
        => db.Seniors.FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<SeniorProfile?> GetWithFamilyAsync(long id, CancellationToken ct = default)
        => db.Seniors.Include(s => s.FamilyMembers).FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<SeniorProfile?> GetFullAsync(long id, CancellationToken ct = default)
        => db.Seniors
            .Include(s => s.FamilyMembers)
            .Include(s => s.Conditions)
            .Include(s => s.Medications)
            .Include(s => s.Allergies)
            .Include(s => s.EmergencyContacts)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<List<SeniorProfile>> GetByFamilyUserAsync(long userId, CancellationToken ct = default)
        => db.Seniors
            .Include(s => s.FamilyMembers)
            .Where(s => s.FamilyMembers.Any(m => m.UserId == userId))
            .ToListAsync(ct);

    public async Task AddAsync(SeniorProfile senior, CancellationToken ct = default)
        => await db.Seniors.AddAsync(senior, ct);

    public Task UpdateAsync(SeniorProfile senior, CancellationToken ct = default)
    {
        db.Seniors.Update(senior);
        return Task.CompletedTask;
    }

    public Task<long> GetNextIdAsync() => Task.FromResult(hiLo.GetNextId<SeniorProfile>());

    public async Task<long> GetNextConditionIdAsync()
        => (await db.MedicalConditions.MaxAsync(c => (long?)c.Id) ?? 0) + 1;

    public async Task<long> GetNextMedicationIdAsync()
        => (await db.Medications.MaxAsync(m => (long?)m.Id) ?? 0) + 1;

    public async Task<long> GetNextAllergyIdAsync()
        => (await db.Allergies.MaxAsync(a => (long?)a.Id) ?? 0) + 1;

    public async Task<long> GetNextContactIdAsync()
        => (await db.EmergencyContacts.MaxAsync(e => (long?)e.Id) ?? 0) + 1;

    public async Task<long> GetNextFamilyMemberIdAsync()
        => (await db.FamilyMembers.MaxAsync(f => (long?)f.Id) ?? 0) + 1;
}

public sealed class ShiftRepository(CareDbContext db, IHiLoIdGenerator hiLo) : IShiftRepository
{
    public Task<CareShift?> GetByIdAsync(long id, CancellationToken ct = default)
        => db.Shifts.FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<List<CareShift>> GetBySeniorAsync(long seniorId, CancellationToken ct = default)
        => db.Shifts
            .Where(s => s.SeniorId == seniorId)
            .OrderByDescending(s => s.Date).ThenByDescending(s => s.StartTime)
            .ToListAsync(ct);

    public Task<List<CareShift>> GetByCaregiverAsync(long caregiverId, DateOnly from, DateOnly to, CancellationToken ct = default)
        => db.Shifts
            .Where(s => s.CaregiverId == caregiverId && s.Date >= from && s.Date <= to)
            .OrderBy(s => s.Date).ThenBy(s => s.StartTime)
            .ToListAsync(ct);

    public Task<List<CareShift>> GetByAgencyAsync(long agencyId, ShiftStatus? status, CancellationToken ct = default)
        => db.Shifts
            .Where(s => s.AgencyId == agencyId && (status == null || s.Status == status))
            .OrderByDescending(s => s.Date)
            .ToListAsync(ct);

    public Task<bool> HasOverlappingShiftAsync(
        long caregiverId, DateOnly date, TimeOnly startTime, TimeOnly endTime,
        long? excludeShiftId = null, CancellationToken ct = default)
        => db.Shifts.AnyAsync(s =>
            s.CaregiverId == caregiverId &&
            s.Date == date &&
            s.Status != ShiftStatus.Cancelled &&
            s.Status != ShiftStatus.NoShow &&
            (excludeShiftId == null || s.Id != excludeShiftId) &&
            s.StartTime < endTime && startTime < s.EndTime, ct);

    public async Task<List<CareShift>> GetUnassignedUpcomingAsync(DateTime until, CancellationToken ct = default)
    {
        var untilDate = DateOnly.FromDateTime(until);
        var untilTime = TimeOnly.FromDateTime(until);

        return await db.Shifts
            .Where(s => s.Status == ShiftStatus.Requested &&
                        (s.Date < untilDate || (s.Date == untilDate && s.StartTime <= untilTime)))
            .OrderBy(s => s.Date).ThenBy(s => s.StartTime)
            .ToListAsync(ct);
    }

    public async Task AddAsync(CareShift shift, CancellationToken ct = default)
        => await db.Shifts.AddAsync(shift, ct);

    public Task UpdateAsync(CareShift shift, CancellationToken ct = default)
    {
        db.Shifts.Update(shift);
        return Task.CompletedTask;
    }

    public Task<List<CareShift>> GetUnpaidCompletedAsync(long agencyId, DateOnly from, DateOnly to, CancellationToken ct = default)
        => db.Shifts
            .Where(s => s.AgencyId == agencyId
                     && s.Status == ShiftStatus.Completed
                     && s.PayoutId == null
                     && s.Date >= from && s.Date <= to)
            .OrderBy(s => s.CaregiverId).ThenBy(s => s.Date)
            .ToListAsync(ct);

    public Task<List<CareShift>> GetByPayoutAsync(long payoutId, CancellationToken ct = default)
        => db.Shifts.Where(s => s.PayoutId == payoutId).ToListAsync(ct);

    public Task<long> GetNextIdAsync() => Task.FromResult(hiLo.GetNextId<CareShift>());
}

public sealed class PayoutRepository(CareDbContext db, IHiLoIdGenerator hiLo) : IPayoutRepository
{
    public Task<CaregiverPayout?> GetByIdAsync(long id, CancellationToken ct = default)
        => db.Payouts.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<List<CaregiverPayout>> GetByAgencyAsync(long agencyId, CancellationToken ct = default)
        => db.Payouts.Where(p => p.AgencyId == agencyId).OrderByDescending(p => p.CreatedAt).ToListAsync(ct);

    public async Task AddAsync(CaregiverPayout payout, CancellationToken ct = default)
        => await db.Payouts.AddAsync(payout, ct);

    public Task UpdateAsync(CaregiverPayout payout, CancellationToken ct = default)
    {
        db.Payouts.Update(payout);
        return Task.CompletedTask;
    }

    public Task<long> GetNextIdAsync() => Task.FromResult(hiLo.GetNextId<CaregiverPayout>());
}

public sealed class ShiftReportRepository(CareDbContext db, IHiLoIdGenerator hiLo) : IShiftReportRepository
{
    public Task<ShiftReport?> GetByIdAsync(long id, CancellationToken ct = default)
        => db.ShiftReports.Include(r => r.MedicationIntakes).FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<ShiftReport?> GetByShiftIdAsync(long shiftId, CancellationToken ct = default)
        => db.ShiftReports.Include(r => r.MedicationIntakes).FirstOrDefaultAsync(r => r.ShiftId == shiftId, ct);

    public Task<List<ShiftReport>> GetRecentBySeniorAsync(long seniorId, int count, CancellationToken ct = default)
        => db.ShiftReports
            .Include(r => r.MedicationIntakes)
            .Where(r => r.SeniorId == seniorId)
            .OrderByDescending(r => r.SubmittedAt)
            .Take(count)
            .ToListAsync(ct);

    public async Task AddAsync(ShiftReport report, CancellationToken ct = default)
        => await db.ShiftReports.AddAsync(report, ct);

    public Task UpdateAsync(ShiftReport report, CancellationToken ct = default)
    {
        db.ShiftReports.Update(report);
        return Task.CompletedTask;
    }

    public Task<long> GetNextIdAsync() => Task.FromResult(hiLo.GetNextId<ShiftReport>());

    public async Task<long> GetNextIntakeIdAsync()
        => (await db.MedicationIntakes.MaxAsync(m => (long?)m.Id) ?? 0) + 1;
}

public sealed class CareOrderRepository(CareDbContext db, IHiLoIdGenerator hiLo) : ICareOrderRepository
{
    public Task<CareOrder?> GetByIdAsync(long id, CancellationToken ct = default)
        => db.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task<CareOrder?> GetByShiftIdAsync(long shiftId, CancellationToken ct = default)
        => db.Orders.FirstOrDefaultAsync(o => o.ShiftId == shiftId, ct);

    public Task<CareOrder?> GetByAuthorityAsync(string authority, CancellationToken ct = default)
        => db.Orders.FirstOrDefaultAsync(o => o.PaymentAuthority == authority, ct);

    public Task<List<CareOrder>> GetBySeniorAsync(long seniorId, CancellationToken ct = default)
        => db.Orders.Where(o => o.SeniorId == seniorId).OrderByDescending(o => o.CreatedAt).ToListAsync(ct);

    public async Task AddAsync(CareOrder order, CancellationToken ct = default)
        => await db.Orders.AddAsync(order, ct);

    public Task UpdateAsync(CareOrder order, CancellationToken ct = default)
    {
        db.Orders.Update(order);
        return Task.CompletedTask;
    }

    public Task<long> GetNextIdAsync() => Task.FromResult(hiLo.GetNextId<CareOrder>());
}
