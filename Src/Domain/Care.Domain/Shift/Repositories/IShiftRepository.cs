using Care.Domain.Shift.Aggregates;
using Care.Domain.Shift.Enums;

namespace Care.Domain.Shift.Repositories;

public interface IShiftRepository
{
    Task<CareShift?>      GetByIdAsync(long id, CancellationToken ct = default);
    Task<List<CareShift>> GetBySeniorAsync(long seniorId, CancellationToken ct = default);
    Task<List<CareShift>> GetByCaregiverAsync(long caregiverId, DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<List<CareShift>> GetByAgencyAsync(long agencyId, ShiftStatus? status, CancellationToken ct = default);

    /// <summary>آیا پرستار در این بازه شیفت دیگری دارد — جلوگیری از تخصیص هم‌پوشان</summary>
    Task<bool>            HasOverlappingShiftAsync(long caregiverId, DateOnly date, TimeOnly startTime, TimeOnly endTime, long? excludeShiftId = null, CancellationToken ct = default);

    /// <summary>شیفت‌هایی که پرستار تخصیص نیافته و زمانشان نزدیک است — پایه‌ی هشدار پوشش</summary>
    Task<List<CareShift>> GetUnassignedUpcomingAsync(DateTime until, CancellationToken ct = default);

    /// <summary>شیفت‌های تکمیل‌شده‌ای که هنوز در هیچ فیش حقوقی حساب نشده‌اند</summary>
    Task<List<CareShift>> GetUnpaidCompletedAsync(long agencyId, DateOnly from, DateOnly to, CancellationToken ct = default);

    /// <summary>شیفت‌های یک فیش حقوقی — برای آزادسازی هنگام لغو فیش</summary>
    Task<List<CareShift>> GetByPayoutAsync(long payoutId, CancellationToken ct = default);

    Task                  AddAsync(CareShift shift, CancellationToken ct = default);
    Task                  UpdateAsync(CareShift shift, CancellationToken ct = default);
    Task<long>            GetNextIdAsync();
}
