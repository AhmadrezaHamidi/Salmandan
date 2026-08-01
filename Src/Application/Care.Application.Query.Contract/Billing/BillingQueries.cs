using Care.Domain.Billing.Enums;

namespace Billing.Application.Queries;

public record GetOrderQuery(long Id) : IQuery<OrderResult?>;

public record GetOrderByShiftQuery(long ShiftId) : IQuery<OrderResult?>;

public record GetOrdersQuery(
    int          Page = 1,
    int          PageSize = 20,
    long?        SeniorId = null,
    long?        AgencyId = null,
    OrderStatus? Status = null) : IQuery<PagedResult<OrderListItem>>;

/// <summary>خلاصه مالی مؤسسه — مبالغ در امانت، آزادشده و کمیسیون پلتفرم</summary>
public record GetAgencyBalanceQuery(long AgencyId) : IQuery<AgencyBalanceResult>;

public sealed record OrderResult(
    long      Id,
    long      ShiftId,
    long      SeniorId,
    long      AgencyId,
    long      PayerUserId,
    decimal   Amount,
    decimal   PlatformFee,
    decimal   AgencyShare,
    string    Status,
    string?   PaymentRefId,
    DateTime? EscrowedAt,
    DateTime? ReleasedAt,
    DateTime? RefundedAt,
    string?   RefundReason,
    bool      IsInvoiceIssued,
    string?   InvoiceNumber,
    DateTime  CreatedAt);

public sealed record OrderListItem(
    long     Id,
    long     ShiftId,
    decimal  Amount,
    string   Status,
    bool     IsInvoiceIssued,
    DateTime CreatedAt);

public sealed record AgencyBalanceResult(
    long    AgencyId,
    decimal EscrowedAmount,
    decimal ReleasedAmount,
    decimal RefundedAmount,
    decimal PlatformFeeTotal,
    int     OrderCount);
