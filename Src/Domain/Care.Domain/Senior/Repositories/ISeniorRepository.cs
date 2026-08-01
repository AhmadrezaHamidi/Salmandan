using Care.Domain.Senior.Aggregates;

namespace Care.Domain.Senior.Repositories;

public interface ISeniorRepository
{
    Task<SeniorProfile?>      GetByIdAsync(long id, CancellationToken ct = default);

    /// <summary>پرونده همراه با اعضای حلقه خانواده — برای بررسی دسترسی</summary>
    Task<SeniorProfile?>      GetWithFamilyAsync(long id, CancellationToken ct = default);

    /// <summary>پرونده کامل با تمام آیتم‌های پزشکی</summary>
    Task<SeniorProfile?>      GetFullAsync(long id, CancellationToken ct = default);

    /// <summary>سالمندانی که کاربر به آن‌ها دسترسی دارد</summary>
    Task<List<SeniorProfile>> GetByFamilyUserAsync(long userId, CancellationToken ct = default);

    Task                      AddAsync(SeniorProfile senior, CancellationToken ct = default);
    Task                      UpdateAsync(SeniorProfile senior, CancellationToken ct = default);
    Task<long>                GetNextIdAsync();
    Task<long>                GetNextConditionIdAsync();
    Task<long>                GetNextMedicationIdAsync();
    Task<long>                GetNextAllergyIdAsync();
    Task<long>                GetNextContactIdAsync();
    Task<long>                GetNextFamilyMemberIdAsync();
}
