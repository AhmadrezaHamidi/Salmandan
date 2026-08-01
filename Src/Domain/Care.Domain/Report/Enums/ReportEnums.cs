namespace Care.Domain.Report.Enums;

/// <summary>مقیاس ساده سه‌سطحی برای شاخص‌های رفتاری — قابل ثبت توسط پرستار غیرمتخصص</summary>
public enum ObservationScale
{
    /// <summary>بد / کم</summary>
    Poor = 1,

    /// <summary>متوسط / معمولی</summary>
    Fair = 2,

    /// <summary>خوب / زیاد</summary>
    Good = 3
}

/// <summary>سطح تحرک سالمند در طول شیفت</summary>
public enum MobilityLevel
{
    /// <summary>بستری، بدون تحرک</summary>
    Bedridden = 1,

    /// <summary>با کمک جابه‌جا می‌شود</summary>
    Assisted = 2,

    /// <summary>با واکر یا عصا</summary>
    WithAid = 3,

    /// <summary>مستقل راه می‌رود</summary>
    Independent = 4
}

/// <summary>نوع حادثه در شیفت — ورودی مستقیم مدل پیش‌بینی ریسک</summary>
public enum IncidentType
{
    /// <summary>سقوط</summary>
    Fall = 1,

    /// <summary>عدم مصرف دارو</summary>
    MissedMedication = 2,

    /// <summary>وخامت ناگهانی حال</summary>
    HealthDeterioration = 3,

    /// <summary>مراجعه به اورژانس یا بستری</summary>
    Hospitalization = 4,

    /// <summary>بی‌قراری یا پرخاشگری</summary>
    Agitation = 5,

    /// <summary>سایر</summary>
    Other = 6
}
