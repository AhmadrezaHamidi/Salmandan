using Msg = Care.Application.Validators.Resources.Message;

namespace Care.Application.Validators.Identity;

public sealed class RequestOtpCommandValidator : AbstractValidator<RequestOtpCommand>
{
    public RequestOtpCommandValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage(Msg.PhoneNumberRequired)
            .Matches(@"^09\d{9}$").WithMessage(Msg.PhoneNumberInvalid);
    }
}

public sealed class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
{
    public VerifyOtpCommandValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage(Msg.PhoneNumberRequired)
            .Matches(@"^09\d{9}$").WithMessage(Msg.PhoneNumberInvalid);

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage(Msg.OtpCodeRequired)
            .Matches(@"^\d{4,6}$").WithMessage(Msg.OtpCodeInvalid);
    }
}

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
        => RuleFor(x => x.RefreshToken).NotEmpty().WithMessage(Msg.RefreshTokenRequired);
}

public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.NationalId)
            .Matches(@"^\d{10}$").WithMessage(Msg.NationalIdInvalid)
            .When(x => !string.IsNullOrWhiteSpace(x.NationalId));
    }
}
