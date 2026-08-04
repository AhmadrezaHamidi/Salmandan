namespace Care.Domain.Payroll.Args;

public sealed record CreatePayoutArg(
    long     Id,
    long     AgencyId,
    long     CaregiverId,
    DateOnly PeriodFrom,
    DateOnly PeriodTo,
    int      ShiftCount,
    decimal  TotalHours,
    decimal  TotalAmount);
