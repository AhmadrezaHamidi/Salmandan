namespace Care.Domain.Payroll.Enums;

/// <summary>وضعیت فیش حقوقی پرستار</summary>
public enum PayoutStatus
{
    /// <summary>محاسبه شده، هنوز پرداخت نشده</summary>
    Pending = 1,

    /// <summary>پرداخت شده به پرستار</summary>
    Paid = 2,

    /// <summary>لغو شده — شیفت‌هایش دوباره قابل محاسبه‌اند</summary>
    Cancelled = 3
}
