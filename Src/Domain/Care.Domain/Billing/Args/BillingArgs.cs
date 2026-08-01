namespace Care.Domain.Billing.Args;

public sealed record CreateOrderArg(
    long    Id,
    long    ShiftId,
    long    SeniorId,
    long    AgencyId,
    long    PayerUserId,
    decimal Amount,
    decimal CommissionPercent);
