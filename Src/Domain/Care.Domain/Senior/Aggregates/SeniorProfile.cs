using Care.Domain.Common.Enums;
using Care.Domain.Senior.Args;
using Care.Domain.Senior.Entities;
using Care.Domain.Senior.Enums;
using Care.Domain.Senior.Exceptions;

namespace Care.Domain.Senior.Aggregates;

/// <summary>
/// Aggregate Root پرونده مراقبت سالمند — قلب سیستم.
/// مرز سازگاری «چه کسی به داده این سالمند دسترسی دارد» را نگه می‌دارد.
/// </summary>
public sealed class SeniorProfile : AggregateRoot<long>
{
    private readonly List<MedicalCondition> _conditions        = [];
    private readonly List<Medication>       _medications       = [];
    private readonly List<Allergy>          _allergies         = [];
    private readonly List<EmergencyContact> _emergencyContacts = [];
    private readonly List<FamilyMember>     _familyMembers     = [];

    public string          FullName             { get; private set; } = string.Empty;
    public string?         NationalId           { get; private set; }
    public DateOnly        BirthDate            { get; private set; }
    public Gender          Gender               { get; private set; }
    public DependencyLevel DependencyLevel      { get; private set; }

    public string          City                 { get; private set; } = string.Empty;
    public string?         District             { get; private set; }
    public string          Address              { get; private set; } = string.Empty;

    /// <summary>مختصات منزل — مبنای چک‌این ژئوفنس پرستار</summary>
    public double?         Latitude             { get; private set; }
    public double?         Longitude            { get; private set; }

    /// <summary>کاربر ایجادکننده پرونده — همیشه Manager است</summary>
    public long            PrimaryFamilyUserId  { get; private set; }

    public string?         Notes                { get; private set; }
    public bool            IsActive             { get; private set; }
    public DateTime        CreatedAt            { get; private set; }

    public IReadOnlyCollection<MedicalCondition> Conditions        => _conditions.AsReadOnly();
    public IReadOnlyCollection<Medication>       Medications       => _medications.AsReadOnly();
    public IReadOnlyCollection<Allergy>          Allergies         => _allergies.AsReadOnly();
    public IReadOnlyCollection<EmergencyContact> EmergencyContacts => _emergencyContacts.AsReadOnly();
    public IReadOnlyCollection<FamilyMember>     FamilyMembers     => _familyMembers.AsReadOnly();

    /// <summary>سن سالمند</summary>
    public int Age
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age   = today.Year - BirthDate.Year;
            if (BirthDate > today.AddYears(-age)) age--;
            return age;
        }
    }

    private SeniorProfile() { }

    private SeniorProfile(CreateSeniorArg arg) : base(arg.Id)
    {
        FullName            = arg.FullName;
        NationalId          = arg.NationalId;
        BirthDate           = arg.BirthDate;
        Gender              = arg.Gender;
        DependencyLevel     = arg.DependencyLevel;
        City                = arg.City;
        District            = arg.District;
        Address             = arg.Address;
        Latitude            = arg.Latitude;
        Longitude           = arg.Longitude;
        PrimaryFamilyUserId = arg.PrimaryFamilyUserId;
        Notes               = arg.Notes;
        IsActive            = true;
        CreatedAt           = DateTime.UtcNow;
    }

    /// <summary>ایجاد پرونده مراقبت — ایجادکننده خودکار Manager حلقه خانواده می‌شود</summary>
    public static SeniorProfile Create(CreateSeniorArg arg)
    {
        var senior = new SeniorProfile(arg);
        senior._familyMembers.Add(new FamilyMember(
            arg.PrimaryMemberId, arg.Id, arg.PrimaryFamilyUserId,
            FamilyRelation.Child, FamilyAccessLevel.Manager, arg.TimeZoneId));

        senior.RaiseDomainEvent(new SeniorProfileCreatedEvent(arg.Id, arg.FullName, arg.PrimaryFamilyUserId));
        return senior;
    }

    /// <summary>بروزرسانی اطلاعات پایه و آدرس</summary>
    public void UpdateProfile(string fullName, string city, string? district, string address,
                              double? latitude, double? longitude, string? notes)
    {
        FullName  = fullName;
        City      = city;
        District  = district;
        Address   = address;
        Latitude  = latitude;
        Longitude = longitude;
        Notes     = notes;
    }

    /// <summary>تغییر سطح وابستگی — بر قیمت و نوع خدمت اثر می‌گذارد</summary>
    public void ChangeDependencyLevel(DependencyLevel level)
    {
        if (DependencyLevel == level) return;

        var old = DependencyLevel;
        DependencyLevel = level;
        RaiseDomainEvent(new SeniorDependencyChangedEvent(Id, old.ToString(), level.ToString()));
    }

    public void Deactivate() => IsActive = false;
    public void Activate()   => IsActive = true;

    // ── حلقه خانواده ──────────────────────────────────────────────────────

    /// <summary>افزودن عضو به حلقه خانواده</summary>
    public void AddFamilyMember(long memberId, long userId, FamilyRelation relation,
                                FamilyAccessLevel accessLevel, string? timeZoneId)
    {
        if (_familyMembers.Any(m => m.UserId == userId))
            throw new FamilyMemberAlreadyExistsException();

        _familyMembers.Add(new FamilyMember(memberId, Id, userId, relation, accessLevel, timeZoneId));
        RaiseDomainEvent(new FamilyMemberAddedEvent(Id, userId, relation.ToString(), accessLevel.ToString()));
    }

    /// <summary>حذف عضو از حلقه خانواده — آخرین Manager قابل حذف نیست</summary>
    public void RemoveFamilyMember(long userId)
    {
        var member = _familyMembers.FirstOrDefault(m => m.UserId == userId)
            ?? throw new FamilyMemberNotFoundException();

        if (member.AccessLevel == FamilyAccessLevel.Manager &&
            _familyMembers.Count(m => m.AccessLevel == FamilyAccessLevel.Manager) == 1)
            throw new CannotRemoveLastManagerException();

        _familyMembers.Remove(member);
        RaiseDomainEvent(new FamilyMemberRemovedEvent(Id, userId));
    }

    /// <summary>تغییر سطح دسترسی عضو</summary>
    public void ChangeFamilyMemberAccess(long userId, FamilyAccessLevel accessLevel)
    {
        var member = _familyMembers.FirstOrDefault(m => m.UserId == userId)
            ?? throw new FamilyMemberNotFoundException();

        if (member.AccessLevel == FamilyAccessLevel.Manager &&
            accessLevel != FamilyAccessLevel.Manager &&
            _familyMembers.Count(m => m.AccessLevel == FamilyAccessLevel.Manager) == 1)
            throw new CannotRemoveLastManagerException();

        member.ChangeAccessLevel(accessLevel);
    }

    /// <summary>آیا کاربر به پرونده دسترسی دارد</summary>
    public bool HasAccess(long userId) => _familyMembers.Any(m => m.UserId == userId);

    /// <summary>آیا کاربر مجاز به رزرو شیفت و پرداخت است</summary>
    public bool CanBookShift(long userId)
        => _familyMembers.Any(m => m.UserId == userId &&
                                   m.AccessLevel != FamilyAccessLevel.Viewer);

    /// <summary>آیا کاربر مدیر پرونده است</summary>
    public bool IsManager(long userId)
        => _familyMembers.Any(m => m.UserId == userId && m.AccessLevel == FamilyAccessLevel.Manager);

    // ── پرونده پزشکی ──────────────────────────────────────────────────────

    /// <summary>افزودن بیماری به پرونده</summary>
    public void AddCondition(long conditionId, string title, string? description, DateOnly? diagnosedAt, bool isChronic)
    {
        if (_conditions.Any(c => c.Title == title))
            throw new DuplicateConditionException();

        _conditions.Add(new MedicalCondition(conditionId, Id, title, description, diagnosedAt, isChronic));
    }

    /// <summary>حذف بیماری از پرونده</summary>
    public void RemoveCondition(long conditionId)
    {
        var condition = _conditions.FirstOrDefault(c => c.Id == conditionId)
            ?? throw new ConditionNotFoundException();
        _conditions.Remove(condition);
    }

    /// <summary>افزودن دارو به پرونده</summary>
    public void AddMedication(long medicationId, string name, string dosage, MedicationFrequency frequency,
                              string? instruction, DateOnly startDate, DateOnly? endDate)
    {
        if (_medications.Any(m => m.Name == name && m.IsActive))
            throw new DuplicateMedicationException();

        _medications.Add(new Medication(medicationId, Id, name, dosage, frequency, instruction, startDate, endDate));
        RaiseDomainEvent(new MedicationAddedEvent(Id, name, dosage));
    }

    /// <summary>قطع مصرف دارو</summary>
    public void DiscontinueMedication(long medicationId)
    {
        var medication = _medications.FirstOrDefault(m => m.Id == medicationId)
            ?? throw new MedicationNotFoundException();
        medication.Discontinue();
    }

    /// <summary>داروهای فعال — مبنای چک‌لیست مصرف دارو در گزارش شیفت</summary>
    public IReadOnlyList<Medication> ActiveMedications
        => _medications.Where(m => m.IsActive).ToList();

    /// <summary>افزودن حساسیت</summary>
    public void AddAllergy(long allergyId, string title, string? reaction)
    {
        if (_allergies.Any(a => a.Title == title))
            throw new DuplicateAllergyException();

        _allergies.Add(new Allergy(allergyId, Id, title, reaction));
    }

    /// <summary>افزودن تماس اضطراری</summary>
    public void AddEmergencyContact(long contactId, string fullName, string phoneNumber, string relation, int priority)
    {
        if (_emergencyContacts.Any(c => c.PhoneNumber == phoneNumber))
            throw new DuplicateEmergencyContactException();

        _emergencyContacts.Add(new EmergencyContact(contactId, Id, fullName, phoneNumber, relation, priority));
    }
}
