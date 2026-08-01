namespace Care.Read.Dapper.Report;

public sealed class ReportReadRepository(IDbConnection conn)
{
    private const string DetailSelect = """
        SELECT r.Id, r.ShiftId, r.SeniorId, sr.FullName AS SeniorName,
               r.CaregiverId, c.FullName AS CaregiverName, s.Date AS ShiftDate,
               r.Mood, r.Appetite, r.SleepQuality, r.Mobility, r.PainLevel,
               r.BloodPressureSystolic, r.BloodPressureDiastolic, r.PulseRate,
               r.Temperature, r.BloodSugar, r.OxygenSaturation,
               r.HasIncident, r.IncidentKind, r.IncidentDescription,
               r.Notes, r.IsApprovedByFamily, r.SubmittedAt
        FROM ShiftReports r
        INNER JOIN Shifts     s  ON s.Id  = r.ShiftId
        INNER JOIN Seniors    sr ON sr.Id = r.SeniorId
        INNER JOIN Caregivers c  ON c.Id  = r.CaregiverId
        """;

    public Task<ShiftReportResult?> GetByIdAsync(long id, CancellationToken ct = default)
        => LoadDetailAsync($"{DetailSelect} WHERE r.Id = @Key AND r.IsDeleted = 0", id, ct);

    public Task<ShiftReportResult?> GetByShiftIdAsync(long shiftId, CancellationToken ct = default)
        => LoadDetailAsync($"{DetailSelect} WHERE r.ShiftId = @Key AND r.IsDeleted = 0", shiftId, ct);

    public async Task<(List<ShiftReportListItem> Items, int Total)> GetPagedBySeniorAsync(
        long seniorId, int page, int pageSize, CancellationToken ct = default)
    {
        var param = new DynamicParameters();
        param.Add("SeniorId", seniorId);
        param.Add("Offset",   (page - 1) * pageSize);
        param.Add("PageSize", pageSize);

        const string countSql = "SELECT COUNT(*) FROM ShiftReports r WHERE r.SeniorId = @SeniorId AND r.IsDeleted = 0";
        const string dataSql  = """
            SELECT r.Id, r.ShiftId, s.Date AS ShiftDate, c.FullName AS CaregiverName,
                   r.Mood, r.Appetite, r.PainLevel, r.HasIncident, r.IsApprovedByFamily, r.SubmittedAt
            FROM ShiftReports r
            INNER JOIN Shifts     s ON s.Id = r.ShiftId
            INNER JOIN Caregivers c ON c.Id = r.CaregiverId
            WHERE r.SeniorId = @SeniorId AND r.IsDeleted = 0
            ORDER BY r.SubmittedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var total = await conn.QuerySingleAsync<int>(countSql, param);
        var items = (await conn.QueryAsync<ShiftReportListItem>(dataSql, param)).ToList();
        return (items, total);
    }

    /// <summary>
    /// روند شاخص‌های سلامت در N گزارش اخیر.
    /// مقیاس‌های متنی به عدد ۱ تا ۳/۴ تبدیل می‌شوند تا مستقیم روی نمودار بنشینند.
    /// </summary>
    public async Task<IReadOnlyList<HealthTimelinePoint>> GetTimelineAsync(
        long seniorId, int count, CancellationToken ct = default)
    {
        const string sql = """
            SELECT TOP (@Count)
                   r.SubmittedAt,
                   s.Date AS ShiftDate,
                   CASE r.Mood         WHEN 'Poor' THEN 1 WHEN 'Fair' THEN 2 ELSE 3 END AS MoodScore,
                   CASE r.Appetite     WHEN 'Poor' THEN 1 WHEN 'Fair' THEN 2 ELSE 3 END AS AppetiteScore,
                   CASE r.SleepQuality WHEN 'Poor' THEN 1 WHEN 'Fair' THEN 2 ELSE 3 END AS SleepScore,
                   CASE r.Mobility     WHEN 'Bedridden' THEN 1 WHEN 'Assisted' THEN 2
                                       WHEN 'WithAid'   THEN 3 ELSE 4 END AS MobilityScore,
                   r.PainLevel, r.BloodPressureSystolic, r.PulseRate, r.HasIncident
            FROM ShiftReports r
            INNER JOIN Shifts s ON s.Id = r.ShiftId
            WHERE r.SeniorId = @SeniorId AND r.IsDeleted = 0
            ORDER BY r.SubmittedAt DESC
            """;
        var result = await conn.QueryAsync<HealthTimelinePoint>(sql, new { SeniorId = seniorId, Count = count });
        return result.Reverse().ToList(); // از قدیم به جدید برای رسم نمودار
    }

    private async Task<ShiftReportResult?> LoadDetailAsync(string headSql, long key, CancellationToken ct)
    {
        var head = await conn.QueryFirstOrDefaultAsync<ReportHeadRow>(headSql, new { Key = key });
        if (head is null) return null;

        const string intakeSql = """
            SELECT i.MedicationId, i.MedicationName, i.WasTaken, i.Reason
            FROM MedicationIntakes i WHERE i.ReportId = @ReportId
            """;
        var intakes = (await conn.QueryAsync<MedicationIntakeItem>(intakeSql, new { ReportId = head.Id })).ToList();

        return new ShiftReportResult(
            head.Id, head.ShiftId, head.SeniorId, head.SeniorName, head.CaregiverId, head.CaregiverName,
            head.ShiftDate, head.Mood, head.Appetite, head.SleepQuality, head.Mobility, head.PainLevel,
            head.BloodPressureSystolic, head.BloodPressureDiastolic, head.PulseRate, head.Temperature,
            head.BloodSugar, head.OxygenSaturation, head.HasIncident, head.IncidentKind,
            head.IncidentDescription, head.Notes, head.IsApprovedByFamily, head.SubmittedAt, intakes);
    }

    private sealed record ReportHeadRow(
        long Id, long ShiftId, long SeniorId, string SeniorName, long CaregiverId, string CaregiverName,
        DateOnly ShiftDate, string Mood, string Appetite, string SleepQuality, string Mobility, int PainLevel,
        int? BloodPressureSystolic, int? BloodPressureDiastolic, int? PulseRate, decimal? Temperature,
        int? BloodSugar, int? OxygenSaturation, bool HasIncident, string? IncidentKind,
        string? IncidentDescription, string? Notes, bool IsApprovedByFamily, DateTime SubmittedAt);
}
