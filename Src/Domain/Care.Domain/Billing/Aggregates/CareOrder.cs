using Care.Domain.Billing.Args;
using Care.Domain.Billing.Enums;
using Care.Domain.Billing.Exceptions;

namespace Care.Domain.Billing.Aggregates;

/// <summary>
/// Aggregate Root سفارش پرداخت شیفت با مدل امانی (Escrow).
/// وجه خانواده تا تأیید گزارش شیفت نزد پلتفرم می‌ماند؛
/// سپس سهم مؤسسه آزاد و کمیسیون پلتفرم کسر می‌شود.
/// </summary>
public sealed class CareOrder : AggregateRoot<long>
{
    public long        ShiftId       { get; private set; }
    public long        SeniorId      { get; private set; }
    public long        AgencyId      { get; private set; }
    public long        PayerUserId   { get; private set; }

    /// <summary>مبلغ کل به ریال</summary>
    public decimal     Amount        { get; private set; }

    /// <summary>کمیسیون پلتفرم</summary>
    public decimal     PlatformFee   { get; private set; }

    /// <summary>سهم مؤسسه</summary>
    public decimal     AgencyShare   { get; private set; }

    public OrderStatus Status        { get; private set; }
    public string?     PaymentRefId  { get; private set; }
    public string?     PaymentAuthority { get; private set; }

    public DateTime?   EscrowedAt    { get; private set; }
    public DateTime?   ReleasedAt    { get; private set; }
    public DateTime?   RefundedAt    { get; private set; }
    public string?     RefundReason  { get; private set; }

    /// <summary>فاکتور رسمی برای ارائه به بیمه تکمیلی صادر شده است؟</summary>
    public bool        IsInvoiceIssued { get; private set; }
    public string?     InvoiceNumber   { get; private set; }

    public DateTime    CreatedAt     { get; private set; }

    private CareOrder() { }

    private CareOrder(CreateOrderArg arg) : base(arg.Id)
    {
        ShiftId     = arg.ShiftId;
        SeniorId    = arg.SeniorId;
        AgencyId    = arg.AgencyId;
        PayerUserId = arg.PayerUserId;
        Amount      = arg.Amount;
        PlatformFee = Math.Round(arg.Amount * arg.CommissionPercent / 100, 0);
        AgencyShare = arg.Amount - PlatformFee;
        Status      = OrderStatus.Pending;
        CreatedAt   = DateTime.UtcNow;
    }

    /// <summary>ایجاد سفارش پرداخت برای یک شیفت</summary>
    public static CareOrder Create(CreateOrderArg arg)
    {
        if (arg.Amount <= 0)
            throw new InvalidOrderAmountException();

        if (arg.CommissionPercent is < 0 or > 100)
            throw new InvalidOrderAmountException();

        var order = new CareOrder(arg);
        order.RaiseDomainEvent(new CareOrderCreatedEvent(arg.Id, arg.ShiftId, arg.PayerUserId, arg.Amount));
        return order;
    }

    /// <summary>ثبت شناسه تراکنش درگاه پیش از پرداخت — وضعیت هنوز Pending می‌ماند</summary>
    public void AttachPaymentAuthority(string authority)
    {
        if (Status != OrderStatus.Pending && Status != OrderStatus.Failed)
            throw new InvalidOrderTransitionException();

        PaymentAuthority = authority;
    }

    /// <summary>ثبت پرداخت موفق — وجه وارد امانت می‌شود</summary>
    public void MarkEscrowed(string paymentRefId, string? authority)
    {
        if (Status == OrderStatus.Escrowed) throw new OrderAlreadyEscrowedException();
        if (Status != OrderStatus.Pending && Status != OrderStatus.Failed)
            throw new InvalidOrderTransitionException();

        Status           = OrderStatus.Escrowed;
        PaymentRefId     = paymentRefId;
        PaymentAuthority = authority;
        EscrowedAt       = DateTime.UtcNow;
        RaiseDomainEvent(new OrderEscrowedEvent(Id, ShiftId, Amount, paymentRefId));
    }

    /// <summary>ثبت پرداخت ناموفق</summary>
    public void MarkFailed()
    {
        if (Status == OrderStatus.Escrowed) throw new OrderAlreadyEscrowedException();
        Status = OrderStatus.Failed;
    }

    /// <summary>آزادسازی وجه به مؤسسه پس از تکمیل شیفت و تأیید گزارش</summary>
    public void Release()
    {
        if (Status == OrderStatus.Released) throw new OrderAlreadyReleasedException();
        if (Status != OrderStatus.Escrowed) throw new OrderNotEscrowedException();

        Status     = OrderStatus.Released;
        ReleasedAt = DateTime.UtcNow;
        RaiseDomainEvent(new OrderReleasedEvent(Id, ShiftId, AgencyShare, PlatformFee));
    }

    /// <summary>عودت وجه به خانواده — لغو شیفت یا عدم حضور پرستار</summary>
    public void Refund(string reason)
    {
        if (Status == OrderStatus.Refunded) throw new OrderAlreadyRefundedException();
        if (Status == OrderStatus.Released) throw new OrderAlreadyReleasedException();
        if (Status != OrderStatus.Escrowed) throw new OrderNotEscrowedException();

        Status       = OrderStatus.Refunded;
        RefundReason = reason;
        RefundedAt   = DateTime.UtcNow;
        RaiseDomainEvent(new OrderRefundedEvent(Id, ShiftId, Amount, reason));
    }

    /// <summary>صدور فاکتور رسمی قابل ارائه به بیمه تکمیلی</summary>
    public void IssueInvoice(string invoiceNumber)
    {
        if (Status != OrderStatus.Released) throw new OrderNotReleasedException();
        if (IsInvoiceIssued)                throw new InvoiceAlreadyIssuedException();

        IsInvoiceIssued = true;
        InvoiceNumber   = invoiceNumber;
    }
}
