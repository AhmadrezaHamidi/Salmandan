using Care.Domain.Report.Aggregates;

namespace Care.Domain.Report.Repositories;

public interface IShiftReportRepository
{
    Task<ShiftReport?>      GetByIdAsync(long id, CancellationToken ct = default);
    Task<ShiftReport?>      GetByShiftIdAsync(long shiftId, CancellationToken ct = default);

    /// <summary>آخرین N گزارش سالمند — ورودی تحلیل روند سلامت</summary>
    Task<List<ShiftReport>> GetRecentBySeniorAsync(long seniorId, int count, CancellationToken ct = default);

    Task                    AddAsync(ShiftReport report, CancellationToken ct = default);
    Task                    UpdateAsync(ShiftReport report, CancellationToken ct = default);
    Task<long>              GetNextIdAsync();
    Task<long>              GetNextIntakeIdAsync();
}
