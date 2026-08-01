using Msg = Care.Application.Validators.Resources.Message;

namespace Care.Application.Validators.Caregiver;

public sealed class RegisterCaregiverCommandValidator : AbstractValidator<RegisterCaregiverCommand>
{
    public RegisterCaregiverCommandValidator()
    {
        RuleFor(x => x.AgencyId).GreaterThan(0).WithMessage(Msg.AgencyIdInvalid);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage(Msg.PhoneNumberRequired)
            .Matches(@"^09\d{9}$").WithMessage(Msg.PhoneNumberInvalid);

        RuleFor(x => x.FullName).NotEmpty().WithMessage(Msg.FullNameRequired).MaximumLength(100);

        RuleFor(x => x.NationalId)
            .NotEmpty().WithMessage(Msg.NationalIdInvalid)
            .Matches(@"^\d{10}$").WithMessage(Msg.NationalIdInvalid);

        RuleFor(x => x.BirthYear)
            .InclusiveBetween(1300, 1420).WithMessage(Msg.BirthYearInvalid);
    }
}

public sealed class SuspendCaregiverCommandValidator : AbstractValidator<SuspendCaregiverCommand>
{
    public SuspendCaregiverCommandValidator()
        => RuleFor(x => x.Reason).NotEmpty().WithMessage(Msg.ReasonRequired).MaximumLength(500);
}

public sealed class RejectCaregiverCommandValidator : AbstractValidator<RejectCaregiverCommand>
{
    public RejectCaregiverCommandValidator()
        => RuleFor(x => x.Reason).NotEmpty().WithMessage(Msg.ReasonRequired).MaximumLength(500);
}

public sealed class AddCaregiverSkillCommandValidator : AbstractValidator<AddCaregiverSkillCommand>
{
    public AddCaregiverSkillCommandValidator()
    {
        RuleFor(x => x.CaregiverId).GreaterThan(0).WithMessage(Msg.CaregiverIdInvalid);
        RuleFor(x => x.YearsOfExperience).InclusiveBetween(0, 60);
    }
}
