namespace Care.Domain.Caregiver.Enums;

/// <summary>سطح صلاحیت پرستار</summary>
public enum CaregiverLevel
{
    /// <summary>همدم / همراه سالمند — بدون مهارت بالینی</summary>
    Companion = 1,

    /// <summary>کمک‌بهیار / مراقب</summary>
    CareAssistant = 2,

    /// <summary>بهیار</summary>
    PracticalNurse = 3,

    /// <summary>پرستار دارای مدرک رسمی</summary>
    RegisteredNurse = 4
}

/// <summary>وضعیت احراز صلاحیت پرستار — پاسخ مستقیم به پدیده «پرستارنما»</summary>
public enum CaregiverStatus
{
    /// <summary>ثبت شده، مدارک بررسی نشده</summary>
    Pending = 1,

    /// <summary>مدارک و سوءپیشینه تأیید شده</summary>
    Verified = 2,

    /// <summary>رد صلاحیت</summary>
    Rejected = 3,

    /// <summary>تعلیق موقت</summary>
    Suspended = 4
}

/// <summary>مهارت‌های تخصصی پرستار</summary>
public enum CaregiverSkillType
{
    /// <summary>مراقبت از بیمار آلزایمری</summary>
    Alzheimer = 1,

    /// <summary>مراقبت از بیمار پارکینسون</summary>
    Parkinson = 2,

    /// <summary>بیمار بستری و زخم بستر</summary>
    Bedridden = 3,

    /// <summary>تزریقات و سرم‌تراپی</summary>
    Injection = 4,

    /// <summary>مراقبت از بیمار دیابتی</summary>
    Diabetes = 5,

    /// <summary>فیزیوتراپی و حرکت‌درمانی</summary>
    Physiotherapy = 6,

    /// <summary>مراقبت پس از سکته</summary>
    PostStroke = 7,

    /// <summary>مراقبت تنفسی و اکسیژن</summary>
    Respiratory = 8
}
