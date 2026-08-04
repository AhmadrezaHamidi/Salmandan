using Care.Domain.Payroll.Enums;

namespace Care.Read.Dapper.Payroll;

public sealed class PayrollReadRepository(IDbConnection conn)
{
    public async Task<PayoutResult?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        const string sql = """
            SELECT p.Id, p.AgencyId, p.CaregiverId, c.FullName AS CaregiverName,
                   p.PeriodFrom, p.PeriodTo, p.ShiftCount, p.TotalHours, p.TotalAmount,
                   CASE WHEN p.TotalHours > 0 THEN ROUND(p.TotalAmount / p.TotalHours, 0) ELSE 0 END AS HourlyRate,
                   p.Status, p.PaidAt, p.PaymentReference, p.CreatedAt
            FROM Payouts p
            INNER JOIN Caregivers c ON c.Id = p.CaregiverId
            WHERE p.Id = @Id AND p.IsDeleted = 0;

            SELECT s.Id AS ShiftId, s.Date, s.StartTime, s.EndTime,
                   CAST(CASE WHEN s.CheckInAt IS NOT NULL AND s.CheckOutAt IS NOT NULL
                             THEN DATEDIFF(MINUTE, s.CheckInAt, s.CheckOutAt) / 60.0
                             ELSE DATEDIFF(MINUTE, CAST(s.StartTime AS datetime), CAST(s.EndTime AS datetime)) / 60.0
                        END AS decimal(9,2)) AS WorkedHours,
                   s.CaregiverPayRate AS Amount
            FROM Shifts s
            WHERE s.PayoutId = @Id
            ORDER BY s.Date, s.StartTime;
            """;

        using var multi = await conn.QueryMultipleAsync(sql, new { Id = id });

        var head = await multi.ReadFirstOrDefaultAsync<PayoutHeadRow>();
        if (head is null) return null;

        var shifts = (await multi.ReadAsync<PayoutShiftItem>()).ToList();

        return new PayoutResult(
            head.Id, head.AgencyId, head.CaregiverId, head.CaregiverName,
            head.PeriodFrom, head.PeriodTo, head.ShiftCount, head.TotalHours, head.TotalAmount,
            head.HourlyRate, head.Status, head.PaidAt, head.PaymentReference, head.CreatedAt, shifts);
    }

    public async Task<(List<PayoutListItem> Items, int Total)> GetPagedAsync(
        int page, int pageSize, long? agencyId, long? caregiverId, PayoutStatus? status, CancellationToken ct = default)
    {
        var where = new List<string> { "p.IsDeleted = 0" };
        var param = new DynamicParameters();

        if (agencyId.HasValue)    { where.Add("p.AgencyId = @AgencyId");       param.Add("AgencyId", agencyId.Value); }
        if (caregiverId.HasValue) { where.Add("p.CaregiverId = @CaregiverId"); param.Add("CaregiverId", caregiverId.Value); }
        if (status.HasValue)      { where.Add("p.Status = @Status");           param.Add("Status", status.Value.ToString()); }

        var whereClause = "WHERE " + string.Join(" AND ", where);
        param.Add("Offset",   (page - 1) * pageSize);
        param.Add("PageSize", pageSize);

        var countSql = $"SELECT COUNT(*) FROM Payouts p {whereClause}";
        var dataSql  = $"""
            SELECT p.Id, p.CaregiverId, c.FullName AS CaregiverName, p.PeriodFrom, p.PeriodTo,
                   p.ShiftCount, p.TotalAmount, p.Status, p.CreatedAt
            FROM Payouts p
            INNER JOIN Caregivers c ON c.Id = p.CaregiverId
            {whereClause}
            ORDER BY p.CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var total = await conn.QuerySingleAsync<int>(countSql, param);
        var items = (await conn.QueryAsync<PayoutListItem>(dataSql, param)).ToList();
        return (items, total);
    }

    /// <summary>پیش‌نمایش حقوق — بدون قفل کردن شیفت‌ها</summary>
    public async Task<IReadOnlyList<PayrollPreviewItem>> PreviewAsync(
        long agencyId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        const string sql = """
            SELECT s.CaregiverId, c.FullName AS CaregiverName,
                   COUNT(*) AS ShiftCount,
                   CAST(SUM(CASE WHEN s.CheckInAt IS NOT NULL AND s.CheckOutAt IS NOT NULL
                                 THEN DATEDIFF(MINUTE, s.CheckInAt, s.CheckOutAt) / 60.0
                                 ELSE DATEDIFF(MINUTE, CAST(s.StartTime AS datetime), CAST(s.EndTime AS datetime)) / 60.0
                            END) AS decimal(9,2)) AS TotalHours,
                   SUM(s.CaregiverPayRate) AS TotalAmount
            FROM Shifts s
            INNER JOIN Caregivers c ON c.Id = s.CaregiverId
            WHERE s.IsDeleted = 0
              AND s.AgencyId = @AgencyId
              AND s.Status   = 'Completed'
              AND s.PayoutId IS NULL
              AND s.Date BETWEEN @From AND @To
            GROUP BY s.CaregiverId, c.FullName
            ORDER BY TotalAmount DESC
            """;
        var result = await conn.QueryAsync<PayrollPreviewItem>(sql, new { AgencyId = agencyId, From = from, To = to });
        return result.ToList();
    }

    /// <summary>سود و زیان مؤسسه در بازه — درآمد، هزینه پرستار، کمیسیون پلتفرم</summary>
    public async Task<AgencyProfitResult> GetProfitAsync(
        long agencyId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        const string sql = """
            SELECT @AgencyId AS AgencyId, @From AS [From], @To AS [To],
                   ISNULL(COUNT(*), 0)                        AS CompletedShifts,
                   ISNULL(SUM(s.Price), 0)                    AS Revenue,
                   ISNULL(SUM(s.CaregiverPayRate), 0)         AS CaregiverCost,
                   ISNULL(SUM(o.PlatformFee), 0)              AS PlatformFee,
                   ISNULL(SUM(s.Price - s.CaregiverPayRate), 0) - ISNULL(SUM(o.PlatformFee), 0) AS GrossProfit,
                   CASE WHEN ISNULL(SUM(s.Price), 0) > 0
                        THEN ROUND((ISNULL(SUM(s.Price - s.CaregiverPayRate), 0) - ISNULL(SUM(o.PlatformFee), 0))
                                   * 100.0 / SUM(s.Price), 1)
                        ELSE 0 END AS MarginPercent
            FROM Shifts s
            LEFT JOIN Orders o ON o.ShiftId = s.Id AND o.Status = 'Released'
            WHERE s.IsDeleted = 0
              AND s.AgencyId = @AgencyId
              AND s.Status   = 'Completed'
              AND s.Date BETWEEN @From AND @To
            """;
        return await conn.QuerySingleAsync<AgencyProfitResult>(sql, new { AgencyId = agencyId, From = from, To = to });
    }

    private sealed record PayoutHeadRow(
        long Id, long AgencyId, long CaregiverId, string CaregiverName,
        DateOnly PeriodFrom, DateOnly PeriodTo, int ShiftCount, decimal TotalHours,
        decimal TotalAmount, decimal HourlyRate, string Status, DateTime? PaidAt,
        string? PaymentReference, DateTime CreatedAt);
}
