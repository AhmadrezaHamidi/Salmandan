using AhmadBase.Doamin;

namespace Care.Domain.Events.Payroll;

/// <summary>رویداد صدور فیش حقوقی پرستار</summary>
public sealed record PayoutGeneratedEvent(long PayoutId, long AgencyId, long CaregiverId, int ShiftCount, decimal TotalAmount) : IEvent;

/// <summary>رویداد پرداخت حقوق — پیامک اطلاع به پرستار</summary>
public sealed record PayoutPaidEvent(long PayoutId, long CaregiverId, decimal TotalAmount, string PaymentReference) : IEvent;
