using Msg = Care.Application.Validators.Resources.Message;

namespace Care.Application.Validators.Report;

public sealed class SubmitShiftReportCommandValidator : AbstractValidator<SubmitShiftReportCommand>
{
    public SubmitShiftReportCommandValidator()
    {
        RuleFor(x => x.ShiftId).GreaterThan(0).WithMessage(Msg.ShiftIdInvalid);

        RuleFor(x => x.PainLevel).InclusiveBetween(0, 10).WithMessage(Msg.PainLevelOutOfRange);

        RuleFor(x => x.BloodPressureSystolic!.Value).InclusiveBetween(50, 260)
            .WithMessage(Msg.BloodPressureOutOfRange).When(x => x.BloodPressureSystolic.HasValue);

        RuleFor(x => x.BloodPressureDiastolic!.Value).InclusiveBetween(30, 180)
            .WithMessage(Msg.BloodPressureOutOfRange).When(x => x.BloodPressureDiastolic.HasValue);

        RuleFor(x => x.PulseRate!.Value).InclusiveBetween(20, 220)
            .WithMessage(Msg.PulseOutOfRange).When(x => x.PulseRate.HasValue);

        RuleFor(x => x.Temperature!.Value).InclusiveBetween(30m, 45m)
            .WithMessage(Msg.TemperatureOutOfRange).When(x => x.Temperature.HasValue);

        RuleFor(x => x.OxygenSaturation!.Value).InclusiveBetween(50, 100)
            .WithMessage(Msg.OxygenOutOfRange).When(x => x.OxygenSaturation.HasValue);
    }
}

public sealed class FlagIncidentCommandValidator : AbstractValidator<FlagIncidentCommand>
{
    public FlagIncidentCommandValidator()
        => RuleFor(x => x.Description).NotEmpty().WithMessage(Msg.IncidentDescriptionRequired).MaximumLength(1000);
}
