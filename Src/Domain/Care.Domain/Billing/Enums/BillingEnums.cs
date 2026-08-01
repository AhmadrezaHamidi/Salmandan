namespace Care.Domain.Billing.Enums;

/// <summary>
/// وضعیت سفارش پرداخت شیفت.
/// Pending → Escrowed → Released — وجه تا تأیید گزارش شیفت در امانت می‌ماند.
/// </summary>
public enum OrderStatus
{
    /// <summary>ایجاد شده، پرداخت نشده</summary>
    Pending = 1,

    /// <summary>پرداخت شده و در امانت پلتفرم</summary>
    Escrowed = 2,

    /// <summary>آزاد شده به مؤسسه</summary>
    Released = 3,

    /// <summary>عودت داده شده به خانواده</summary>
    Refunded = 4,

    /// <summary>پرداخت ناموفق</summary>
    Failed = 5
}
