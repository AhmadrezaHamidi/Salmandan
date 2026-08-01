namespace Care.Domain.Agency.Enums;

/// <summary>وضعیت مؤسسه پرستاری در پلتفرم</summary>
public enum AgencyStatus
{
    /// <summary>ثبت شده، در انتظار بررسی مدارک و مجوز</summary>
    Pending = 1,

    /// <summary>تأیید شده و فعال</summary>
    Active = 2,

    /// <summary>تعلیق موقت (تخلف، انقضای مجوز)</summary>
    Suspended = 3,

    /// <summary>رد شده</summary>
    Rejected = 4
}

/// <summary>نوع خدمت قابل ارائه — تفکیک مراقبتی از بالینی برای رعایت الزام مجوز</summary>
public enum CareServiceType
{
    /// <summary>مراقبت و همراهی سالمند (نیازمند مجوز درمانی نیست)</summary>
    Companionship = 1,

    /// <summary>مراقبت شخصی — بهداشت فردی، پوشک، حمام</summary>
    PersonalCare = 2,

    /// <summary>پرستاری بالینی — تزریق، سرم، پانسمان (نیازمند مجوز وزارت بهداشت)</summary>
    ClinicalNursing = 3,

    /// <summary>فیزیوتراپی و توانبخشی در منزل</summary>
    Physiotherapy = 4
}
