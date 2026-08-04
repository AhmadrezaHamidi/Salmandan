using Care.Domain.Payroll.Aggregates;

namespace Care.Domain.Payroll.Repositories;

public interface IPayoutRepository
{
    Task<CaregiverPayout?>      GetByIdAsync(long id, CancellationToken ct = default);
    Task<List<CaregiverPayout>> GetByAgencyAsync(long agencyId, CancellationToken ct = default);
    Task                        AddAsync(CaregiverPayout payout, CancellationToken ct = default);
    Task                        UpdateAsync(CaregiverPayout payout, CancellationToken ct = default);
    Task<long>                  GetNextIdAsync();
}
