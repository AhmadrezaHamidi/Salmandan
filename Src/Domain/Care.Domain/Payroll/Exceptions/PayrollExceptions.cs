using Msg = Care.Domain.Resources.Message;

namespace Care.Domain.Payroll.Exceptions;

public sealed class PayoutNotFoundException        : BusinessException { public PayoutNotFoundException()        : base(Msg.PayoutNotFoundException) { } }
public sealed class PayoutAlreadyPaidException     : BusinessException { public PayoutAlreadyPaidException()     : base(Msg.PayoutAlreadyPaidException) { } }
public sealed class PayoutCancelledException       : BusinessException { public PayoutCancelledException()       : base(Msg.PayoutCancelledException) { } }
public sealed class InvalidPayoutPeriodException   : BusinessException { public InvalidPayoutPeriodException()   : base(Msg.InvalidPayoutPeriodException) { } }
public sealed class NoShiftsToPayException         : BusinessException { public NoShiftsToPayException()         : base(Msg.NoShiftsToPayException) { } }
public sealed class PaymentReferenceRequiredException : BusinessException { public PaymentReferenceRequiredException() : base(Msg.PaymentReferenceRequiredException) { } }
