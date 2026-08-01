using Care.Domain.Billing.Enums;

namespace Care.Read.Dapper.Billing;

public sealed class BillingReadRepository(IDbConnection conn)
{
    private const string DetailSelect = """
        SELECT o.Id, o.ShiftId, o.SeniorId, o.AgencyId, o.PayerUserId,
               o.Amount, o.PlatformFee, o.AgencyShare, o.Status, o.PaymentRefId,
               o.EscrowedAt, o.ReleasedAt, o.RefundedAt, o.RefundReason,
               o.IsInvoiceIssued, o.InvoiceNumber, o.CreatedAt
        FROM Orders o
        """;

    public async Task<OrderResult?> GetByIdAsync(long id, CancellationToken ct = default)
        => await conn.QueryFirstOrDefaultAsync<OrderResult>(
            $"{DetailSelect} WHERE o.Id = @Id AND o.IsDeleted = 0", new { Id = id });

    public async Task<OrderResult?> GetByShiftIdAsync(long shiftId, CancellationToken ct = default)
        => await conn.QueryFirstOrDefaultAsync<OrderResult>(
            $"{DetailSelect} WHERE o.ShiftId = @ShiftId AND o.IsDeleted = 0", new { ShiftId = shiftId });

    public async Task<(List<OrderListItem> Items, int Total)> GetPagedAsync(
        int page, int pageSize, long? seniorId, long? agencyId, OrderStatus? status, CancellationToken ct = default)
    {
        var where = new List<string> { "o.IsDeleted = 0" };
        var param = new DynamicParameters();

        if (seniorId.HasValue) { where.Add("o.SeniorId = @SeniorId"); param.Add("SeniorId", seniorId.Value); }
        if (agencyId.HasValue) { where.Add("o.AgencyId = @AgencyId"); param.Add("AgencyId", agencyId.Value); }
        if (status.HasValue)   { where.Add("o.Status = @Status");     param.Add("Status", status.Value.ToString()); }

        var whereClause = "WHERE " + string.Join(" AND ", where);
        param.Add("Offset",   (page - 1) * pageSize);
        param.Add("PageSize", pageSize);

        var countSql = $"SELECT COUNT(*) FROM Orders o {whereClause}";
        var dataSql  = $"""
            SELECT o.Id, o.ShiftId, o.Amount, o.Status, o.IsInvoiceIssued, o.CreatedAt
            FROM Orders o {whereClause}
            ORDER BY o.CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var total = await conn.QuerySingleAsync<int>(countSql, param);
        var items = (await conn.QueryAsync<OrderListItem>(dataSql, param)).ToList();
        return (items, total);
    }

    /// <summary>وضعیت مالی مؤسسه — چقدر در امانت است، چقدر آزاد شده</summary>
    public async Task<AgencyBalanceResult> GetAgencyBalanceAsync(long agencyId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT @AgencyId AS AgencyId,
                   ISNULL(SUM(CASE WHEN o.Status = 'Escrowed' THEN o.AgencyShare ELSE 0 END), 0) AS EscrowedAmount,
                   ISNULL(SUM(CASE WHEN o.Status = 'Released' THEN o.AgencyShare ELSE 0 END), 0) AS ReleasedAmount,
                   ISNULL(SUM(CASE WHEN o.Status = 'Refunded' THEN o.Amount      ELSE 0 END), 0) AS RefundedAmount,
                   ISNULL(SUM(CASE WHEN o.Status = 'Released' THEN o.PlatformFee ELSE 0 END), 0) AS PlatformFeeTotal,
                   COUNT(*) AS OrderCount
            FROM Orders o
            WHERE o.AgencyId = @AgencyId AND o.IsDeleted = 0
            """;
        return await conn.QuerySingleAsync<AgencyBalanceResult>(sql, new { AgencyId = agencyId });
    }
}
