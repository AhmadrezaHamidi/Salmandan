using Care.Domain.Senior.Enums;

namespace Care.Domain.Senior.Entities;

/// <summary>بیماری یا شرایط پزشکی ثبت‌شده در پرونده سالمند</summary>
public sealed class MedicalCondition
{
    public long      Id          { get; private set; }
    public long      SeniorId    { get; private set; }
    public string    Title       { get; private set; } = string.Empty;
    public string?   Description { get; private set; }
    public DateOnly? DiagnosedAt { get; private set; }
    public bool      IsChronic   { get; private set; }

    private MedicalCondition() { }

    public MedicalCondition(long id, long seniorId, string title, string? description, DateOnly? diagnosedAt, bool isChronic)
    {
        Id          = id;
        SeniorId    = seniorId;
        Title       = title;
        Description = description;
        DiagnosedAt = diagnosedAt;
        IsChronic   = isChronic;
    }
}

/// <summary>داروی جاری سالمند — مبنای یادآور دارو و ثبت مصرف در گزارش شیفت</summary>
public sealed class Medication
{
    public long                Id            { get; private set; }
    public long                SeniorId      { get; private set; }
    public string              Name          { get; private set; } = string.Empty;
    public string              Dosage        { get; private set; } = string.Empty;
    public MedicationFrequency Frequency     { get; private set; }
    public string?             Instruction   { get; private set; }
    public DateOnly            StartDate     { get; private set; }
    public DateOnly?           EndDate       { get; private set; }
    public bool                IsActive      { get; private set; }

    private Medication() { }

    public Medication(long id, long seniorId, string name, string dosage, MedicationFrequency frequency,
                      string? instruction, DateOnly startDate, DateOnly? endDate)
    {
        Id          = id;
        SeniorId    = seniorId;
        Name        = name;
        Dosage      = dosage;
        Frequency   = frequency;
        Instruction = instruction;
        StartDate   = startDate;
        EndDate     = endDate;
        IsActive    = true;
    }

    public void Discontinue()
    {
        IsActive = false;
        EndDate  = DateOnly.FromDateTime(DateTime.UtcNow);
    }
}

/// <summary>حساسیت دارویی یا غذایی</summary>
public sealed class Allergy
{
    public long   Id       { get; private set; }
    public long   SeniorId { get; private set; }
    public string Title    { get; private set; } = string.Empty;
    public string? Reaction { get; private set; }

    private Allergy() { }

    public Allergy(long id, long seniorId, string title, string? reaction)
    {
        Id       = id;
        SeniorId = seniorId;
        Title    = title;
        Reaction = reaction;
    }
}

/// <summary>تماس اضطراری — پزشک معالج، همسایه، اورژانس شخصی</summary>
public sealed class EmergencyContact
{
    public long   Id          { get; private set; }
    public long   SeniorId    { get; private set; }
    public string FullName    { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string Relation    { get; private set; } = string.Empty;
    public int    Priority    { get; private set; }

    private EmergencyContact() { }

    public EmergencyContact(long id, long seniorId, string fullName, string phoneNumber, string relation, int priority)
    {
        Id          = id;
        SeniorId    = seniorId;
        FullName    = fullName;
        PhoneNumber = phoneNumber;
        Relation    = relation;
        Priority    = priority;
    }
}

/// <summary>
/// عضو حلقه خانواده — چند فرزند، هرکدام با سطح دسترسی مشخص.
/// پشتیبانی از فرزند مقیم خارج با TimeZone اختصاصی.
/// </summary>
public sealed class FamilyMember
{
    public long              Id          { get; private set; }
    public long              SeniorId    { get; private set; }
    public long              UserId      { get; private set; }
    public FamilyRelation    Relation    { get; private set; }
    public FamilyAccessLevel AccessLevel { get; private set; }

    /// <summary>منطقه زمانی عضو — برای فرزند مقیم خارج از کشور</summary>
    public string?           TimeZoneId  { get; private set; }
    public DateTime          JoinedAt    { get; private set; }

    private FamilyMember() { }

    public FamilyMember(long id, long seniorId, long userId, FamilyRelation relation,
                        FamilyAccessLevel accessLevel, string? timeZoneId)
    {
        Id          = id;
        SeniorId    = seniorId;
        UserId      = userId;
        Relation    = relation;
        AccessLevel = accessLevel;
        TimeZoneId  = timeZoneId;
        JoinedAt    = DateTime.UtcNow;
    }

    public void ChangeAccessLevel(FamilyAccessLevel level) => AccessLevel = level;

    public void SetTimeZone(string? timeZoneId) => TimeZoneId = timeZoneId;
}
