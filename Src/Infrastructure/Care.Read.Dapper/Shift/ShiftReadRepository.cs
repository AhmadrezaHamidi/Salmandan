using Care.Domain.Shift.Enums;

namespace Care.Read.Dapper.Shift;

public sealed class ShiftReadRepository(IDbConnection conn)
{
    private const string ListSelect = """
        SELECT s.Id, s.SeniorId, sr.FullName AS SeniorName, s.CaregiverId, c.FullName AS CaregiverName,
               s.ShiftKind, s.Date, s.StartTime, s.EndTime, s.Status, s.Price
        FROM Shifts s
        INNER JOIN Seniors sr ON sr.Id = s.SeniorId
        LEFT  JOIN Caregivers c ON c.Id = s.CaregiverId
        """;

    public async Task<ShiftResult?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        const string sql = """
            SELECT s.Id, s.SeniorId, sr.FullName AS SeniorName, s.AgencyId, a.Name AS AgencyName,
                   s.CaregiverId, c.FullName AS CaregiverName,
                   s.ServiceType, s.ShiftKind, s.Date, s.StartTime, s.EndTime, s.Status, s.Price,
                   s.AssignedAt, s.CheckInAt, s.CheckOutAt, s.CompletedAt,
                   s.CancelReason, s.CancelledBy, s.ReplacesShiftId, s.FamilyNote, s.CreatedAt
            FROM Shifts s
            INNER JOIN Seniors  sr ON sr.Id = s.SeniorId
            INNER JOIN Agencies a  ON a.Id  = s.AgencyId
            LEFT  JOIN Caregivers c ON c.Id = s.CaregiverId
            WHERE s.Id = @Id AND s.IsDeleted = 0
            """;
        return await conn.QueryFirstOrDefaultAsync<ShiftResult>(sql, new { Id = id });
    }

    public async Task<(List<ShiftListItem> Items, int Total)> GetPagedAsync(
        int page, int pageSize, long? seniorId, long? agencyId, long? caregiverId,
        ShiftStatus? status, DateOnly? from, DateOnly? to, CancellationToken ct = default)
    {
        var where = new List<string> { "s.IsDeleted = 0" };
        var param = new DynamicParameters();

        if (seniorId.HasValue)    { where.Add("s.SeniorId = @SeniorId");       param.Add("SeniorId", seniorId.Value); }
        if (agencyId.HasValue)    { where.Add("s.AgencyId = @AgencyId");       param.Add("AgencyId", agencyId.Value); }
        if (caregiverId.HasValue) { where.Add("s.CaregiverId = @CaregiverId"); param.Add("CaregiverId", caregiverId.Value); }
        if (status.HasValue)      { where.Add("s.Status = @Status");           param.Add("Status", status.Value.ToString()); }
        if (from.HasValue)        { where.Add("s.Date >= @From");              param.Add("From", from.Value); }
        if (to.HasValue)          { where.Add("s.Date <= @To");                param.Add("To", to.Value); }

        var whereClause = "WHERE " + string.Join(" AND ", where);
        param.Add("Offset",   (page - 1) * pageSize);
        param.Add("PageSize", pageSize);

        var countSql = $"SELECT COUNT(*) FROM Shifts s {whereClause}";
        var dataSql  = $"""
            {ListSelect} {whereClause}
            ORDER BY s.Date DESC, s.StartTime DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var total = await conn.QuerySingleAsync<int>(countSql, param);
        var items = (await conn.QueryAsync<ShiftListItem>(dataSql, param)).ToList();
        return (items, total);
    }

    public async Task<IReadOnlyList<ShiftListItem>> GetCalendarAsync(
        long seniorId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var sql = $"""
            {ListSelect}
            WHERE s.IsDeleted = 0 AND s.SeniorId = @SeniorId AND s.Date BETWEEN @From AND @To
            ORDER BY s.Date, s.StartTime
            """;
        var result = await conn.QueryAsync<ShiftListItem>(sql, new { SeniorId = seniorId, From = from, To = to });
        return result.ToList();
    }

    /// <summary>
    /// شیفت‌های بدون پرستار که تا N ساعت آینده شروع می‌شوند.
    /// داشبورد ریسک پوشش — قبل از اینکه خانواده متوجه شود، مؤسسه هشدار می‌گیرد.
    /// </summary>
    public async Task<IReadOnlyList<CoverageRiskItem>> GetCoverageRiskAsync(int withinHours, CancellationToken ct = default)
    {
        const string sql = """
            SELECT s.Id AS ShiftId, s.SeniorId, sr.FullName AS SeniorName,
                   s.AgencyId, a.Name AS AgencyName, s.Date, s.StartTime,
                   CAST(DATEDIFF(MINUTE, GETUTCDATE(), CAST(s.Date AS datetime) + CAST(s.StartTime AS datetime)) / 60.0 AS float) AS HoursUntilStart
            FROM Shifts s
            INNER JOIN Seniors  sr ON sr.Id = s.SeniorId
            INNER JOIN Agencies a  ON a.Id  = s.AgencyId
            WHERE s.IsDeleted = 0
              AND s.Status IN ('Requested', 'NoShow')
              AND CAST(s.Date AS datetime) + CAST(s.StartTime AS datetime)
                  BETWEEN GETUTCDATE() AND DATEADD(HOUR, @WithinHours, GETUTCDATE())
            ORDER BY HoursUntilStart
            """;
        var result = await conn.QueryAsync<CoverageRiskItem>(sql, new { WithinHours = withinHours });
        return result.ToList();
    }
}
