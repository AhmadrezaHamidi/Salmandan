using Care.Domain.Caregiver.Aggregates;

namespace Care.Domain.Caregiver.Repositories;

public interface ICaregiverRepository
{
    Task<CaregiverProfile?>      GetByIdAsync(long id, CancellationToken ct = default);
    Task<CaregiverProfile?>      GetByUserIdAsync(long userId, CancellationToken ct = default);
    Task<bool>                   NationalIdExistsAsync(string nationalId, CancellationToken ct = default);
    Task<List<CaregiverProfile>> GetVerifiedByAgencyAsync(long agencyId, CancellationToken ct = default);

    /// <summary>پرستاران آزاد یک مؤسسه در بازه زمانی — پایه‌ی جایگزین اضطراری</summary>
    Task<List<CaregiverProfile>> GetAvailableAsync(long agencyId, DateOnly date, TimeOnly startTime, TimeOnly endTime, CancellationToken ct = default);

    Task                         AddAsync(CaregiverProfile caregiver, CancellationToken ct = default);
    Task                         UpdateAsync(CaregiverProfile caregiver, CancellationToken ct = default);
    Task<long>                   GetNextIdAsync();
    Task<long>                   GetNextSkillIdAsync();
}
