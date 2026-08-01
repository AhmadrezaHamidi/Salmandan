namespace Care.Read.Dapper.Senior;

public sealed class SeniorReadRepository(IDbConnection conn)
{
    /// <summary>بررسی دسترسی کاربر به پرونده — قبل از هر Query اجرا می‌شود</summary>
    public async Task<bool> HasAccessAsync(long seniorId, long userId, CancellationToken ct = default)
    {
        const string sql = "SELECT COUNT(1) FROM FamilyMembers WHERE SeniorId = @SeniorId AND UserId = @UserId";
        return await conn.ExecuteScalarAsync<int>(sql, new { SeniorId = seniorId, UserId = userId }) > 0;
    }

    public async Task<SeniorResult?> GetFullAsync(long id, CancellationToken ct = default)
    {
        const string sql = """
            SELECT s.Id, s.FullName, s.NationalId, s.BirthDate, s.Gender, s.DependencyLevel,
                   s.City, s.District, s.Address, s.Latitude, s.Longitude, s.Notes, s.IsActive, s.CreatedAt
            FROM Seniors s
            WHERE s.Id = @Id AND s.IsDeleted = 0;

            SELECT c.Id, c.Title, c.Description, c.DiagnosedAt, c.IsChronic
            FROM MedicalConditions c WHERE c.SeniorId = @Id;

            SELECT m.Id, m.Name, m.Dosage, m.Frequency, m.Instruction, m.StartDate, m.EndDate, m.IsActive
            FROM Medications m WHERE m.SeniorId = @Id;

            SELECT a.Id, a.Title, a.Reaction
            FROM Allergies a WHERE a.SeniorId = @Id;

            SELECT e.Id, e.FullName, e.PhoneNumber, e.Relation, e.Priority
            FROM EmergencyContacts e WHERE e.SeniorId = @Id ORDER BY e.Priority;

            SELECT f.UserId,
                   ISNULL(NULLIF(LTRIM(RTRIM(ISNULL(u.FirstName,'') + ' ' + ISNULL(u.LastName,''))), ''), u.PhoneNumber) AS DisplayName,
                   u.PhoneNumber, f.Relation, f.AccessLevel, f.TimeZoneId, f.JoinedAt
            FROM FamilyMembers f
            INNER JOIN Users u ON u.Id = f.UserId
            WHERE f.SeniorId = @Id;
            """;

        using var multi = await conn.QueryMultipleAsync(sql, new { Id = id });

        var head = await multi.ReadFirstOrDefaultAsync<SeniorHeadRow>();
        if (head is null) return null;

        var conditions = (await multi.ReadAsync<ConditionItem>()).ToList();
        var medications = (await multi.ReadAsync<MedicationItem>()).ToList();
        var allergies = (await multi.ReadAsync<AllergyItem>()).ToList();
        var contacts = (await multi.ReadAsync<EmergencyContactItem>()).ToList();
        var family = (await multi.ReadAsync<FamilyMemberItem>()).ToList();

        return new SeniorResult(
            head.Id, head.FullName, head.NationalId, head.BirthDate, CalculateAge(head.BirthDate),
            head.Gender, head.DependencyLevel, head.City, head.District, head.Address,
            head.Latitude, head.Longitude, head.Notes, head.IsActive, head.CreatedAt,
            conditions, medications, allergies, contacts, family);
    }

    public async Task<IReadOnlyList<SeniorListItem>> GetByFamilyUserAsync(long userId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT s.Id, s.FullName, s.BirthDate, s.Gender, s.DependencyLevel, s.City,
                   f.AccessLevel AS MyAccessLevel, s.IsActive
            FROM Seniors s
            INNER JOIN FamilyMembers f ON f.SeniorId = s.Id AND f.UserId = @UserId
            WHERE s.IsDeleted = 0
            ORDER BY s.FullName
            """;
        var rows = await conn.QueryAsync<SeniorListRow>(sql, new { UserId = userId });
        return rows.Select(r => new SeniorListItem(
            r.Id, r.FullName, CalculateAge(r.BirthDate), r.Gender,
            r.DependencyLevel, r.City, r.MyAccessLevel, r.IsActive)).ToList();
    }

    public async Task<IReadOnlyList<FamilyMemberItem>> GetFamilyCircleAsync(long seniorId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT f.UserId,
                   ISNULL(NULLIF(LTRIM(RTRIM(ISNULL(u.FirstName,'') + ' ' + ISNULL(u.LastName,''))), ''), u.PhoneNumber) AS DisplayName,
                   u.PhoneNumber, f.Relation, f.AccessLevel, f.TimeZoneId, f.JoinedAt
            FROM FamilyMembers f
            INNER JOIN Users u ON u.Id = f.UserId
            WHERE f.SeniorId = @SeniorId
            ORDER BY f.JoinedAt
            """;
        var result = await conn.QueryAsync<FamilyMemberItem>(sql, new { SeniorId = seniorId });
        return result.ToList();
    }

    /// <summary>کارت اورژانس — خلاصه‌ی حیاتی پرونده در یک کوئری</summary>
    public async Task<EmergencyCardResult?> GetEmergencyCardAsync(long seniorId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT s.Id, s.FullName, s.BirthDate, s.Gender, s.Address, s.DependencyLevel
            FROM Seniors s WHERE s.Id = @Id AND s.IsDeleted = 0;

            SELECT c.Title FROM MedicalConditions c WHERE c.SeniorId = @Id;

            SELECT m.Name + ' - ' + m.Dosage FROM Medications m WHERE m.SeniorId = @Id AND m.IsActive = 1;

            SELECT a.Title FROM Allergies a WHERE a.SeniorId = @Id;

            SELECT e.Id, e.FullName, e.PhoneNumber, e.Relation, e.Priority
            FROM EmergencyContacts e WHERE e.SeniorId = @Id ORDER BY e.Priority;
            """;

        using var multi = await conn.QueryMultipleAsync(sql, new { Id = seniorId });

        var head = await multi.ReadFirstOrDefaultAsync<EmergencyCardRow>();
        if (head is null) return null;

        var conditions  = (await multi.ReadAsync<string>()).ToList();
        var medications = (await multi.ReadAsync<string>()).ToList();
        var allergies   = (await multi.ReadAsync<string>()).ToList();
        var contacts    = (await multi.ReadAsync<EmergencyContactItem>()).ToList();

        return new EmergencyCardResult(
            head.Id, head.FullName, CalculateAge(head.BirthDate), head.Gender,
            head.Address, head.DependencyLevel, conditions, medications, allergies, contacts);
    }

    private static int CalculateAge(DateOnly birthDate)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age   = today.Year - birthDate.Year;
        if (birthDate > today.AddYears(-age)) age--;
        return age;
    }

    private sealed record SeniorHeadRow(
        long Id, string FullName, string? NationalId, DateOnly BirthDate, string Gender,
        string DependencyLevel, string City, string? District, string Address,
        double? Latitude, double? Longitude, string? Notes, bool IsActive, DateTime CreatedAt);

    private sealed record SeniorListRow(
        long Id, string FullName, DateOnly BirthDate, string Gender,
        string DependencyLevel, string City, string MyAccessLevel, bool IsActive);

    private sealed record EmergencyCardRow(
        long Id, string FullName, DateOnly BirthDate, string Gender, string Address, string DependencyLevel);
}
