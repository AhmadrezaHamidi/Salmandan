namespace Billing.Application.Commands;

/// <summary>ایجاد سفارش پرداخت برای شیفت و دریافت لینک درگاه</summary>
public record CreateCareOrderCommand(
    long ShiftId,
    [property: JsonIgnore] long PayerUserId) : ICommand<CreateOrderResponse>;

/// <summary>تأیید پرداخت پس از بازگشت از درگاه — وجه وارد امانت می‌شود</summary>
public record ConfirmPaymentCommand(string Authority) : ICommand<bool>;

/// <summary>آزادسازی وجه به مؤسسه پس از تأیید گزارش شیفت</summary>
public record ReleaseOrderCommand([property: JsonIgnore] long OrderId) : ICommand<bool>;

/// <summary>عودت وجه به خانواده</summary>
public record RefundOrderCommand([property: JsonIgnore] long OrderId, string Reason) : ICommand<bool>;

/// <summary>صدور فاکتور رسمی قابل ارائه به بیمه تکمیلی</summary>
public record IssueInvoiceCommand([property: JsonIgnore] long OrderId) : ICommand<string>;

public sealed record CreateOrderResponse(long OrderId, decimal Amount, string PaymentUrl, string Authority);
