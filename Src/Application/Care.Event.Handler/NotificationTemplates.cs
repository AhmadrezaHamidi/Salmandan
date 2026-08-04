namespace Care.Event.Handler;

/// <summary>
/// متن پیامک‌هایی که به خانواده می‌رود.
/// کوتاه نگه داشته شده چون هر ۷۰ کاراکتر فارسی یک پیامک حساب می‌شود.
/// </summary>
public static class NotificationTemplates
{
    /// <summary>پرستار در محل حاضر شد — اولین لحظه‌ی «خیالم راحت شد»</summary>
    public static string CaregiverArrived(DateTime atUtc)
        => $"سلام. پرستار ساعت {ToTehranTime(atUtc):HH:mm} در محل حاضر شد و مراقبت شروع شد.";

    /// <summary>گزارش پایان شیفت آماده است</summary>
    public const string ReportReady =
        "گزارش امروز مراقبت ثبت شد. برای دیدن جزئیات وارد اپ سالمندان شوید.";

    /// <summary>حادثه — فوری و بدون تعارف</summary>
    public static string Incident(string description)
        => $"توجه: در شیفت امروز حادثه‌ای ثبت شد. {Trim(description, 90)}";

    /// <summary>تخصیص پرستار به شیفت</summary>
    public const string CaregiverAssigned =
        "پرستار شیفت شما مشخص شد. جزئیات در اپ سالمندان قابل مشاهده است.";

    /// <summary>ایران منطقه زمانی ثابت +۳:۳۰ دارد</summary>
    private static DateTime ToTehranTime(DateTime utc) => utc.AddHours(3).AddMinutes(30);

    private static string Trim(string text, int max)
        => text.Length <= max ? text : text[..max] + "…";
}
