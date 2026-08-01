namespace Care.Domain.Shift.Enums;

/// <summary>نوع شیفت — منطبق بر تعرفه‌های واقعی بازار ایران</summary>
public enum ShiftKind
{
    /// <summary>ساعتی</summary>
    Hourly = 1,

    /// <summary>روزانه (غیر شبانه‌روزی)</summary>
    Daily = 2,

    /// <summary>شب‌کاری</summary>
    Night = 3,

    /// <summary>شبانه‌روزی ۲۴ ساعته</summary>
    FullDay = 4,

    /// <summary>مقیم (ماهانه)</summary>
    LiveIn = 5
}

/// <summary>
/// چرخه حیات شیفت مراقبت.
/// Requested → Assigned → CheckedIn → CheckedOut → Completed
/// </summary>
public enum ShiftStatus
{
    /// <summary>ثبت شده توسط خانواده، منتظر تخصیص پرستار توسط مؤسسه</summary>
    Requested = 1,

    /// <summary>پرستار تخصیص یافته</summary>
    Assigned = 2,

    /// <summary>پرستار در محل حاضر شده (چک‌این)</summary>
    CheckedIn = 3,

    /// <summary>پرستار خارج شده، منتظر گزارش و تأیید</summary>
    CheckedOut = 4,

    /// <summary>تکمیل شده و وجه آزاد شده</summary>
    Completed = 5,

    /// <summary>لغو شده</summary>
    Cancelled = 6,

    /// <summary>پرستار حاضر نشد — تریگر تضمین پوشش شیفت</summary>
    NoShow = 7
}

/// <summary>لغوکننده شیفت — تعیین‌کننده جریمه و عودت وجه</summary>
public enum CancelActor
{
    Family = 1,
    Agency = 2,
    Platform = 3
}
