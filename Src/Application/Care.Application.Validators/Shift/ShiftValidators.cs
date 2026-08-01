using Msg = Care.Application.Validators.Resources.Message;

namespace Care.Application.Validators.Shift;

public sealed class RequestShiftCommandValidator : AbstractValidator<RequestShiftCommand>
{
    public RequestShiftCommandValidator()
    {
        RuleFor(x => x.SeniorId).GreaterThan(0).WithMessage(Msg.SeniorIdInvalid);
        RuleFor(x => x.AgencyId).GreaterThan(0).WithMessage(Msg.AgencyIdInvalid);

        RuleFor(x => x.Date)
            .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage(Msg.ShiftDateInPast);

        // شیفت شب می‌تواند از نیمه‌شب عبور کند؛ فقط برابری رد می‌شود
        RuleFor(x => x.EndTime).NotEqual(x => x.StartTime).WithMessage(Msg.ShiftEndBeforeStart);
    }
}

public sealed class AssignCaregiverCommandValidator : AbstractValidator<AssignCaregiverCommand>
{
    public AssignCaregiverCommandValidator()
    {
        RuleFor(x => x.ShiftId).GreaterThan(0).WithMessage(Msg.ShiftIdInvalid);
        RuleFor(x => x.CaregiverId).GreaterThan(0).WithMessage(Msg.CaregiverIdInvalid);
    }
}

public sealed class CheckInShiftCommandValidator : AbstractValidator<CheckInShiftCommand>
{
    public CheckInShiftCommandValidator()
    {
        RuleFor(x => x.ShiftId).GreaterThan(0).WithMessage(Msg.ShiftIdInvalid);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).WithMessage(Msg.LatitudeInvalid);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).WithMessage(Msg.LongitudeInvalid);
    }
}

public sealed class ReplaceShiftCommandValidator : AbstractValidator<ReplaceShiftCommand>
{
    public ReplaceShiftCommandValidator()
    {
        RuleFor(x => x.ShiftId).GreaterThan(0).WithMessage(Msg.ShiftIdInvalid);
        RuleFor(x => x.ReplacementCaregiverId).GreaterThan(0).WithMessage(Msg.CaregiverIdInvalid);
    }
}
