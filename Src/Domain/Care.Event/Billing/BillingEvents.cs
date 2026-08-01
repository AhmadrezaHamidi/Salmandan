using AhmadBase.Doamin;

namespace Care.Domain.Events.Billing;

/// <summary>رویداد ایجاد سفارش پرداخت شیفت</summary>
public sealed record CareOrderCreatedEvent(long OrderId, long ShiftId, long PayerUserId, decimal Amount) : IEvent;

/// <summary>رویداد قرار گرفتن وجه در امانت (Escrow)</summary>
public sealed record OrderEscrowedEvent(long OrderId, long ShiftId, decimal Amount, string PaymentRefId) : IEvent;

/// <summary>رویداد آزادسازی وجه به مؤسسه پس از تأیید گزارش شیفت</summary>
public sealed record OrderReleasedEvent(long OrderId, long ShiftId, decimal AgencyShare, decimal PlatformFee) : IEvent;

/// <summary>رویداد عودت وجه به خانواده</summary>
public sealed record OrderRefundedEvent(long OrderId, long ShiftId, decimal Amount, string Reason) : IEvent;
