using Care.Domain.Agency.Enums;

namespace Care.Read.Dapper.Agency;

public sealed class AgencyReadRepository(IDbConnection conn)
{
    public async Task<AgencyResult?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        const string sql = """
            SELECT a.Id, a.Name, a.LicenseNumber, a.LicenseExpiryDate,
                   CAST(CASE WHEN a.LicenseExpiryDate >= CAST(GETUTCDATE() AS date) THEN 1 ELSE 0 END AS bit) AS HasValidLicense,
                   a.OwnerUserId, a.City, a.District, a.Address, a.PhoneNumber, a.Status,
                   a.CommissionPercent, a.RatingAvg, a.RatingCount, a.CompletedShifts, a.CreatedAt
            FROM Agencies a
            WHERE a.Id = @Id AND a.IsDeleted = 0
            """;
        return await conn.QueryFirstOrDefaultAsync<AgencyResult>(sql, new { Id = id });
    }

    public async Task<(List<AgencyListItem> Items, int Total)> GetPagedAsync(
        int page, int pageSize, string? city, AgencyStatus? status, CancellationToken ct = default)
    {
        var where = new List<string> { "a.IsDeleted = 0" };
        var param = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(city)) { where.Add("a.City = @City");     param.Add("City", city); }
        if (status.HasValue)                  { where.Add("a.Status = @Status"); param.Add("Status", status.Value.ToString()); }

        var whereClause = "WHERE " + string.Join(" AND ", where);
        param.Add("Offset",   (page - 1) * pageSize);
        param.Add("PageSize", pageSize);

        var countSql = $"SELECT COUNT(*) FROM Agencies a {whereClause}";
        var dataSql  = $"""
            SELECT a.Id, a.Name, a.City, a.District, a.Status, a.RatingAvg, a.CompletedShifts, a.LicenseExpiryDate
            FROM Agencies a {whereClause}
            ORDER BY a.RatingAvg DESC, a.CompletedShifts DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var total = await conn.QuerySingleAsync<int>(countSql, param);
        var items = (await conn.QueryAsync<AgencyListItem>(dataSql, param)).ToList();
        return (items, total);
    }

    /// <summary>مؤسسات فعال با مجوز معتبر که برای این خدمت تعرفه ثبت کرده‌اند</summary>
    public async Task<IReadOnlyList<AgencyListItem>> GetAvailableAsync(
        string city, CareServiceType serviceType, CancellationToken ct = default)
    {
        const string sql = """
            SELECT DISTINCT a.Id, a.Name, a.City, a.District, a.Status, a.RatingAvg, a.CompletedShifts, a.LicenseExpiryDate
            FROM Agencies a
            INNER JOIN AgencyServicePrices p ON p.AgencyId = a.Id AND p.ServiceType = @ServiceType
            WHERE a.IsDeleted = 0
              AND a.City = @City
              AND a.Status = 'Active'
              AND a.LicenseExpiryDate >= CAST(GETUTCDATE() AS date)
            ORDER BY a.RatingAvg DESC
            """;
        var result = await conn.QueryAsync<AgencyListItem>(sql, new { City = city, ServiceType = serviceType.ToString() });
        return result.ToList();
    }

    public async Task<IReadOnlyList<AgencyPriceItem>> GetPricesAsync(long agencyId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT p.Id, p.ServiceType, p.ShiftKind, p.Price
            FROM AgencyServicePrices p
            WHERE p.AgencyId = @AgencyId
            ORDER BY p.ServiceType, p.ShiftKind
            """;
        var result = await conn.QueryAsync<AgencyPriceItem>(sql, new { AgencyId = agencyId });
        return result.ToList();
    }
}
