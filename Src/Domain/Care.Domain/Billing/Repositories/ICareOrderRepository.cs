using Care.Domain.Billing.Aggregates;

namespace Care.Domain.Billing.Repositories;

public interface ICareOrderRepository
{
    Task<CareOrder?>      GetByIdAsync(long id, CancellationToken ct = default);
    Task<CareOrder?>      GetByShiftIdAsync(long shiftId, CancellationToken ct = default);
    Task<CareOrder?>      GetByAuthorityAsync(string authority, CancellationToken ct = default);
    Task<List<CareOrder>> GetBySeniorAsync(long seniorId, CancellationToken ct = default);
    Task                  AddAsync(CareOrder order, CancellationToken ct = default);
    Task                  UpdateAsync(CareOrder order, CancellationToken ct = default);
    Task<long>            GetNextIdAsync();
}
