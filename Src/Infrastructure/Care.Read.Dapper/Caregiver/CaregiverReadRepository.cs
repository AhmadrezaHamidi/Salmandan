using Care.Domain.Caregiver.Enums;

namespace Care.Read.Dapper.Caregiver;

public sealed class CaregiverReadRepository(IDbConnection conn)
{
    public async Task<CaregiverResult?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        const string sql = """
            SELECT c.Id, c.UserId, c.AgencyId, a.Name AS AgencyName, c.FullName, c.NationalId,
                   c.Gender, c.BirthYear, c.Level, c.Status,
                   c.HasCriminalRecordCheck, c.HasCertificate, c.HasLiabilityInsurance,
                   c.RatingAvg, c.RatingCount, c.CompletedShifts, c.NoShowCount, c.CreatedAt
            FROM Caregivers c
            INNER JOIN Agencies a ON a.Id = c.AgencyId
            WHERE c.Id = @Id AND c.IsDeleted = 0
            """;
        return await conn.QueryFirstOrDefaultAsync<CaregiverResult>(sql, new { Id = id });
    }

    public async Task<(List<CaregiverListItem> Items, int Total)> GetPagedAsync(
        int page, int pageSize, long? agencyId, CaregiverStatus? status, CancellationToken ct = default)
    {
        var where = new List<string> { "c.IsDeleted = 0" };
        var param = new DynamicParameters();

        if (agencyId.HasValue) { where.Add("c.AgencyId = @AgencyId"); param.Add("AgencyId", agencyId.Value); }
        if (status.HasValue)   { where.Add("c.Status = @Status");     param.Add("Status", status.Value.ToString()); }

        var whereClause = "WHERE " + string.Join(" AND ", where);
        param.Add("Offset",   (page - 1) * pageSize);
        param.Add("PageSize", pageSize);

        var countSql = $"SELECT COUNT(*) FROM Caregivers c {whereClause}";
        var dataSql  = $"""
            SELECT c.Id, c.FullName, c.Gender, c.Level, c.Status, c.RatingAvg, c.CompletedShifts
            FROM Caregivers c {whereClause}
            ORDER BY c.RatingAvg DESC, c.CompletedShifts DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var total = await conn.QuerySingleAsync<int>(countSql, param);
        var items = (await conn.QueryAsync<CaregiverListItem>(dataSql, param)).ToList();
        return (items, total);
    }

    /// <summary>
    /// پرستاران تأییدشده‌ای که در بازه خواسته‌شده شیفت هم‌پوشان ندارند.
    /// موتور پیدا کردن جایگزین در سناریوی عدم حضور.
    /// </summary>
    public async Task<IReadOnlyList<CaregiverListItem>> GetAvailableAsync(
        long agencyId, DateOnly date, TimeOnly startTime, TimeOnly endTime, CancellationToken ct = default)
    {
        const string sql = """
            SELECT c.Id, c.FullName, c.Gender, c.Level, c.Status, c.RatingAvg, c.CompletedShifts
            FROM Caregivers c
            WHERE c.IsDeleted = 0
              AND c.AgencyId = @AgencyId
              AND c.Status   = 'Verified'
              AND NOT EXISTS (
                  SELECT 1 FROM Shifts s
                  WHERE s.CaregiverId = c.Id
                    AND s.Date        = @Date
                    AND s.Status NOT IN ('Cancelled', 'NoShow')
                    AND s.StartTime < @EndTime
                    AND @StartTime  < s.EndTime
              )
            ORDER BY c.RatingAvg DESC, c.CompletedShifts DESC
            """;
        var result = await conn.QueryAsync<CaregiverListItem>(sql,
            new { AgencyId = agencyId, Date = date, StartTime = startTime, EndTime = endTime });
        return result.ToList();
    }
}
