using Msg = Care.Application.Validators.Resources.Message;

namespace Care.Application.Validators.Billing;

public sealed class CreateCareOrderCommandValidator : AbstractValidator<CreateCareOrderCommand>
{
    public CreateCareOrderCommandValidator()
        => RuleFor(x => x.ShiftId).GreaterThan(0).WithMessage(Msg.ShiftIdInvalid);
}

public sealed class ConfirmPaymentCommandValidator : AbstractValidator<ConfirmPaymentCommand>
{
    public ConfirmPaymentCommandValidator()
        => RuleFor(x => x.Authority).NotEmpty().WithMessage(Msg.AuthorityRequired);
}

public sealed class RefundOrderCommandValidator : AbstractValidator<RefundOrderCommand>
{
    public RefundOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).GreaterThan(0).WithMessage(Msg.OrderIdInvalid);
        RuleFor(x => x.Reason).NotEmpty().WithMessage(Msg.ReasonRequired).MaximumLength(500);
    }
}
