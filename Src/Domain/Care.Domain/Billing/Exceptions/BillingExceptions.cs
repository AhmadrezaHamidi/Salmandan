using Msg = Care.Domain.Resources.Message;

namespace Care.Domain.Billing.Exceptions;

public sealed class OrderNotFoundException          : BusinessException { public OrderNotFoundException()          : base(Msg.OrderNotFoundException) { } }
public sealed class OrderAlreadyEscrowedException   : BusinessException { public OrderAlreadyEscrowedException()   : base(Msg.OrderAlreadyEscrowedException) { } }
public sealed class OrderAlreadyReleasedException   : BusinessException { public OrderAlreadyReleasedException()   : base(Msg.OrderAlreadyReleasedException) { } }
public sealed class OrderAlreadyRefundedException   : BusinessException { public OrderAlreadyRefundedException()   : base(Msg.OrderAlreadyRefundedException) { } }
public sealed class OrderNotEscrowedException       : BusinessException { public OrderNotEscrowedException()       : base(Msg.OrderNotEscrowedException) { } }
public sealed class OrderNotReleasedException       : BusinessException { public OrderNotReleasedException()       : base(Msg.OrderNotReleasedException) { } }
public sealed class InvalidOrderAmountException     : BusinessException { public InvalidOrderAmountException()     : base(Msg.InvalidOrderAmountException) { } }
public sealed class InvalidOrderTransitionException : BusinessException { public InvalidOrderTransitionException() : base(Msg.InvalidOrderTransitionException) { } }
public sealed class InvoiceAlreadyIssuedException   : BusinessException { public InvoiceAlreadyIssuedException()   : base(Msg.InvoiceAlreadyIssuedException) { } }
public sealed class PaymentFailedException          : BusinessException { public PaymentFailedException()          : base(Msg.PaymentFailedException) { } }
