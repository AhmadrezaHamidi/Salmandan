using Msg = Care.Application.Validators.Resources.Message;

namespace Care.Application.Validators.Agency;

public sealed class RegisterAgencyCommandValidator : AbstractValidator<RegisterAgencyCommand>
{
    public RegisterAgencyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(Msg.AgencyNameRequired).MaximumLength(150);

        RuleFor(x => x.LicenseNumber).NotEmpty().WithMessage(Msg.LicenseNumberRequired).MaximumLength(50);

        RuleFor(x => x.LicenseExpiryDate)
            .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage(Msg.LicenseExpiryInPast);

        RuleFor(x => x.City).NotEmpty().WithMessage(Msg.CityRequired).MaximumLength(50);

        RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage(Msg.PhoneNumberRequired);
    }
}

public sealed class SetAgencyCommissionCommandValidator : AbstractValidator<SetAgencyCommissionCommand>
{
    public SetAgencyCommissionCommandValidator()
        => RuleFor(x => x.CommissionPercent).InclusiveBetween(0, 100).WithMessage(Msg.CommissionOutOfRange);
}

public sealed class SetAgencyServicePriceCommandValidator : AbstractValidator<SetAgencyServicePriceCommand>
{
    public SetAgencyServicePriceCommandValidator()
    {
        RuleFor(x => x.AgencyId).GreaterThan(0).WithMessage(Msg.AgencyIdInvalid);
        RuleFor(x => x.Price).GreaterThan(0).WithMessage(Msg.PriceMustBePositive);
    }
}

public sealed class SuspendAgencyCommandValidator : AbstractValidator<SuspendAgencyCommand>
{
    public SuspendAgencyCommandValidator()
        => RuleFor(x => x.Reason).NotEmpty().WithMessage(Msg.ReasonRequired).MaximumLength(500);
}

public sealed class RejectAgencyCommandValidator : AbstractValidator<RejectAgencyCommand>
{
    public RejectAgencyCommandValidator()
        => RuleFor(x => x.Reason).NotEmpty().WithMessage(Msg.ReasonRequired).MaximumLength(500);
}
