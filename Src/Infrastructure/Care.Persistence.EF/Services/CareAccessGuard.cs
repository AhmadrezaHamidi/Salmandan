using Care.Application.Contract.Services;
using Care.Domain.Identity.Exceptions;

namespace Care.Persistence.EF.Services;

/// <summary>
/// کنترل دسترسی سطح رکورد.
///
/// قاعده‌ها:
/// • ادمین پلتفرم: همه‌چیز
/// • خانواده: فقط سالمندانی که عضو حلقه‌شان است
/// • پرستار: فقط سالمندانی که برایشان شیفت دارد
/// • مدیر مؤسسه: فقط سالمندانی که مؤسسه‌اش برایشان شیفت دارد
/// </summary>
public sealed class CareAccessGuard(CareDbContext db) : ICareAccessGuard
{
    private const string Admin         = "Admin";
    private const string Family        = "Family";
    private const string Caregiver     = "Caregiver";
    private const string AgencyManager = "AgencyManager";

    public async Task EnsureCanViewSeniorAsync(long seniorId, long userId, string? role, CancellationToken ct = default)
    {
        if (role == Admin) return;
        if (await HasSeniorAccessAsync(seniorId, userId, role, ct)) return;

        throw new AccessDeniedException();
    }

    public async Task EnsureCanViewShiftAsync(long shiftId, long userId, string? role, CancellationToken ct = default)
    {
        if (role == Admin) return;

        var shift = await db.Shifts
            .Where(s => s.Id == shiftId)
            .Select(s => new { s.SeniorId, s.AgencyId, s.CaregiverId })
            .FirstOrDefaultAsync(ct);

        // شیفت ناموجود هم «دسترسی ندارید» می‌دهد تا وجود/عدم وجود رکورد لو نرود
        if (shift is null) throw new AccessDeniedException();

        var allowed = role switch
        {
            Family        => await db.FamilyMembers.AnyAsync(f => f.SeniorId == shift.SeniorId && f.UserId == userId, ct),
            Caregiver     => shift.CaregiverId.HasValue &&
                             await db.Caregivers.AnyAsync(c => c.Id == shift.CaregiverId.Value && c.UserId == userId, ct),
            AgencyManager => await db.Agencies.AnyAsync(a => a.Id == shift.AgencyId && a.OwnerUserId == userId, ct),
            _             => false
        };

        if (!allowed) throw new AccessDeniedException();
    }

    public async Task EnsureOwnsAgencyAsync(long agencyId, long userId, string? role, CancellationToken ct = default)
    {
        if (role == Admin) return;

        if (role != AgencyManager ||
            !await db.Agencies.AnyAsync(a => a.Id == agencyId && a.OwnerUserId == userId, ct))
            throw new AccessDeniedException();
    }

    public async Task EnsureCanViewPayoutAsync(long agencyId, long caregiverId, long userId, string? role, CancellationToken ct = default)
    {
        if (role == Admin) return;

        var allowed = role switch
        {
            AgencyManager => await db.Agencies.AnyAsync(a => a.Id == agencyId && a.OwnerUserId == userId, ct),
            Caregiver     => await db.Caregivers.AnyAsync(c => c.Id == caregiverId && c.UserId == userId, ct),
            _             => false
        };

        if (!allowed) throw new AccessDeniedException();
    }

    private Task<bool> HasSeniorAccessAsync(long seniorId, long userId, string? role, CancellationToken ct) => role switch
    {
        Family => db.FamilyMembers.AnyAsync(f => f.SeniorId == seniorId && f.UserId == userId, ct),

        // پرستار فقط تا وقتی شیفتی برای این سالمند دارد
        Caregiver => db.Shifts.AnyAsync(s => s.SeniorId == seniorId &&
                                             s.CaregiverId != null &&
                                             db.Caregivers.Any(c => c.Id == s.CaregiverId && c.UserId == userId), ct),

        AgencyManager => db.Shifts.AnyAsync(s => s.SeniorId == seniorId &&
                                                 db.Agencies.Any(a => a.Id == s.AgencyId && a.OwnerUserId == userId), ct),

        _ => Task.FromResult(false)
    };
}
