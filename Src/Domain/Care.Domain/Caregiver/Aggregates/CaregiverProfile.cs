using Care.Domain.Caregiver.Args;
using Care.Domain.Caregiver.Entities;
using Care.Domain.Caregiver.Enums;
using Care.Domain.Caregiver.Exceptions;
using Care.Domain.Common.Enums;

namespace Care.Domain.Caregiver.Aggregates;

/// <summary>
/// Aggregate Root پرستار / مراقب.
/// وابسته به یک مؤسسه است — پلتفرم مستقیم پرستار استخدام نمی‌کند.
/// </summary>
public sealed class CaregiverProfile : AggregateRoot<long>
{
    private readonly List<CaregiverSkill> _skills = [];

    public long            UserId                 { get; private set; }
    public long            AgencyId               { get; private set; }
    public string          FullName               { get; private set; } = string.Empty;
    public string          NationalId             { get; private set; } = string.Empty;
    public Gender          Gender                 { get; private set; }
    public int             BirthYear              { get; private set; }
    public CaregiverLevel  Level                  { get; private set; }
    public CaregiverStatus Status                 { get; private set; }

    /// <summary>گواهی عدم سوءپیشینه ارائه شده است؟</summary>
    public bool            HasCriminalRecordCheck { get; private set; }

    /// <summary>مدرک تحصیلی / گواهینامه مهارت ارائه شده است؟</summary>
    public bool            HasCertificate         { get; private set; }

    /// <summary>بیمه مسئولیت حرفه‌ای دارد؟</summary>
    public bool            HasLiabilityInsurance  { get; private set; }

    public decimal         RatingAvg              { get; private set; }
    public int             RatingCount            { get; private set; }
    public int             CompletedShifts        { get; private set; }
    public int             NoShowCount            { get; private set; }
    public string?         StatusReason           { get; private set; }
    public DateTime        CreatedAt              { get; private set; }

    public IReadOnlyCollection<CaregiverSkill> Skills => _skills.AsReadOnly();

    /// <summary>آیا پرستار می‌تواند به شیفت تخصیص یابد</summary>
    public bool CanBeAssigned => Status == CaregiverStatus.Verified;

    private CaregiverProfile() { }

    private CaregiverProfile(CreateCaregiverArg arg) : base(arg.Id)
    {
        UserId     = arg.UserId;
        AgencyId   = arg.AgencyId;
        FullName   = arg.FullName;
        NationalId = arg.NationalId;
        Gender     = arg.Gender;
        BirthYear  = arg.BirthYear;
        Level      = arg.Level;
        Status     = CaregiverStatus.Pending;
        CreatedAt  = DateTime.UtcNow;
    }

    /// <summary>ثبت پرستار جدید توسط مؤسسه — در انتظار احراز صلاحیت</summary>
    public static CaregiverProfile Register(CreateCaregiverArg arg)
    {
        var caregiver = new CaregiverProfile(arg);
        caregiver.RaiseDomainEvent(new CaregiverRegisteredEvent(arg.Id, arg.AgencyId, arg.FullName));
        return caregiver;
    }

    /// <summary>
    /// احراز صلاحیت پرستار.
    /// بدون گواهی عدم سوءپیشینه تأیید ممکن نیست — سنگ‌بنای اعتماد پلتفرم.
    /// </summary>
    public void Verify()
    {
        if (Status == CaregiverStatus.Verified) throw new CaregiverAlreadyVerifiedException();
        if (!HasCriminalRecordCheck)            throw new CriminalRecordCheckRequiredException();

        Status       = CaregiverStatus.Verified;
        StatusReason = null;
        RaiseDomainEvent(new CaregiverVerifiedEvent(Id, AgencyId));
    }

    /// <summary>رد صلاحیت پرستار</summary>
    public void Reject(string reason)
    {
        Status       = CaregiverStatus.Rejected;
        StatusReason = reason;
        RaiseDomainEvent(new CaregiverRejectedEvent(Id, reason));
    }

    /// <summary>تعلیق پرستار</summary>
    public void Suspend(string reason)
    {
        if (Status == CaregiverStatus.Suspended) throw new CaregiverAlreadySuspendedException();

        Status       = CaregiverStatus.Suspended;
        StatusReason = reason;
        RaiseDomainEvent(new CaregiverSuspendedEvent(Id, reason));
    }

    /// <summary>ثبت مدارک احراز هویت و صلاحیت</summary>
    public void SubmitDocuments(bool criminalRecordCheck, bool certificate, bool liabilityInsurance)
    {
        HasCriminalRecordCheck = criminalRecordCheck;
        HasCertificate         = certificate;
        HasLiabilityInsurance  = liabilityInsurance;
    }

    /// <summary>بروزرسانی اطلاعات پایه</summary>
    public void UpdateProfile(string fullName, CaregiverLevel level)
    {
        FullName = fullName;
        Level    = level;
    }

    /// <summary>افزودن مهارت تخصصی</summary>
    public void AddSkill(long skillId, CaregiverSkillType skillType, int yearsOfExperience)
    {
        if (_skills.Any(s => s.SkillType == skillType))
            throw new DuplicateSkillException();

        _skills.Add(new CaregiverSkill(skillId, Id, skillType, yearsOfExperience));
    }

    /// <summary>حذف مهارت</summary>
    public void RemoveSkill(CaregiverSkillType skillType)
    {
        var skill = _skills.FirstOrDefault(s => s.SkillType == skillType)
            ?? throw new SkillNotFoundException();
        _skills.Remove(skill);
    }

    /// <summary>ثبت امتیاز خانواده پس از تکمیل شیفت</summary>
    public void AddRating(int score)
    {
        if (score is < 1 or > 5) throw new InvalidCaregiverRatingException();

        RatingAvg   = ((RatingAvg * RatingCount) + score) / (RatingCount + 1);
        RatingCount += 1;
    }

    public void IncrementCompletedShifts() => CompletedShifts += 1;

    /// <summary>ثبت عدم حضور — پس از ۳ بار پرستار خودکار تعلیق می‌شود</summary>
    public void RegisterNoShow()
    {
        NoShowCount += 1;
        if (NoShowCount >= 3 && Status == CaregiverStatus.Verified)
            Suspend("تعلیق خودکار به دلیل ۳ بار عدم حضور در شیفت");
    }
}
