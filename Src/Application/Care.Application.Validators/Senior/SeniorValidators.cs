using Msg = Care.Application.Validators.Resources.Message;

namespace Care.Application.Validators.Senior;

public sealed class CreateSeniorCommandValidator : AbstractValidator<CreateSeniorCommand>
{
    public CreateSeniorCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().WithMessage(Msg.FullNameRequired).MaximumLength(100);

        RuleFor(x => x.BirthDate)
            .LessThan(_ => DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage(Msg.BirthDateInvalid)
            .GreaterThan(new DateOnly(1900, 1, 1)).WithMessage(Msg.BirthDateInvalid);

        RuleFor(x => x.City).NotEmpty().WithMessage(Msg.CityRequired).MaximumLength(50);
        RuleFor(x => x.Address).NotEmpty().WithMessage(Msg.AddressRequired).MaximumLength(300);

        RuleFor(x => x.NationalId)
            .Matches(@"^\d{10}$").WithMessage(Msg.NationalIdInvalid)
            .When(x => !string.IsNullOrWhiteSpace(x.NationalId));

        RuleFor(x => x.Latitude!.Value).InclusiveBetween(-90, 90)
            .WithMessage(Msg.LatitudeInvalid).When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude!.Value).InclusiveBetween(-180, 180)
            .WithMessage(Msg.LongitudeInvalid).When(x => x.Longitude.HasValue);
    }
}

public sealed class AddFamilyMemberCommandValidator : AbstractValidator<AddFamilyMemberCommand>
{
    public AddFamilyMemberCommandValidator()
    {
        RuleFor(x => x.SeniorId).GreaterThan(0).WithMessage(Msg.SeniorIdInvalid);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage(Msg.PhoneNumberRequired)
            .Matches(@"^09\d{9}$").WithMessage(Msg.PhoneNumberInvalid);
    }
}

public sealed class AddMedicationCommandValidator : AbstractValidator<AddMedicationCommand>
{
    public AddMedicationCommandValidator()
    {
        RuleFor(x => x.SeniorId).GreaterThan(0).WithMessage(Msg.SeniorIdInvalid);
        RuleFor(x => x.Name).NotEmpty().WithMessage(Msg.MedicationNameRequired).MaximumLength(150);
        RuleFor(x => x.Dosage).NotEmpty().WithMessage(Msg.DosageRequired).MaximumLength(100);
    }
}

public sealed class AddConditionCommandValidator : AbstractValidator<AddConditionCommand>
{
    public AddConditionCommandValidator()
    {
        RuleFor(x => x.SeniorId).GreaterThan(0).WithMessage(Msg.SeniorIdInvalid);
        RuleFor(x => x.Title).NotEmpty().WithMessage(Msg.ConditionTitleRequired).MaximumLength(150);
    }
}

public sealed class AddEmergencyContactCommandValidator : AbstractValidator<AddEmergencyContactCommand>
{
    public AddEmergencyContactCommandValidator()
    {
        RuleFor(x => x.SeniorId).GreaterThan(0).WithMessage(Msg.SeniorIdInvalid);
        RuleFor(x => x.FullName).NotEmpty().WithMessage(Msg.FullNameRequired).MaximumLength(100);
        RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage(Msg.PhoneNumberRequired);
    }
}
