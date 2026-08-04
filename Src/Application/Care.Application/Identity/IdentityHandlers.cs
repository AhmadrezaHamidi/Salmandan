using Care.Domain.Identity.Aggregates;
using Care.Domain.Identity.Args;
using Care.Domain.Identity.Entities;
using Care.Domain.Identity.Exceptions;
using Microsoft.Extensions.Configuration;

namespace Care.Application.Identity;

/// <summary>Handler های احراز هویت OTP و مدیریت پروفایل</summary>
public sealed class IdentityHandlers(
    IUserRepository         userRepo,
    IOtpRepository          otpRepo,
    IRefreshTokenRepository refreshTokenRepo,
    ISmsService             smsService,
    IJwtService             jwtService,
    IConfiguration          configuration,
    CareDbContext           db) :
    ICommandHandler<RequestOtpCommand, RequestOtpResponse>,
    ICommandHandler<VerifyOtpCommand, LoginResponse>,
    ICommandHandler<RefreshTokenCommand, LoginResponse>,
    ICommandHandler<LogoutCommand, bool>,
    ICommandHandler<UpdateProfileCommand, bool>
{
    public async Task<RequestOtpResponse> Handle(RequestOtpCommand command, CancellationToken token)
    {
        var code = AhmadBase.Helper.Random.Randomizer.GenerateOtp();
        var id   = await otpRepo.GetNextIdAsync();
        var otp  = new OtpRequest(id, command.PhoneNumber, code);

        await otpRepo.AddAsync(otp, token);
        await db.SaveChangesAsync(token);

        await smsService.SendOtpAsync(command.PhoneNumber, code, token);

        // در محیط توسعه کد در پاسخ برگردانده می‌شود تا تست دستی ممکن باشد
        var isFake = bool.TryParse(configuration["UseFakeServices"], out var b) && b;
        return new RequestOtpResponse(isFake ? $"SMS_UNAVAILABLE:{code}" : "ok");
    }

    public async Task<LoginResponse> Handle(VerifyOtpCommand command, CancellationToken token)
    {
        var otp = await otpRepo.GetLatestByPhoneAsync(command.PhoneNumber, token)
            ?? throw new OtpNotFoundException();

        otp.Verify(command.Code);
        await otpRepo.UpdateAsync(otp, token);

        var user = await userRepo.GetByPhoneAsync(command.PhoneNumber, token);
        if (user is null)
        {
            var id   = await userRepo.GetNextIdAsync();
            var role = IsPlatformAdmin(command.PhoneNumber)
                ? Domain.Identity.Enums.UserRole.Admin
                : Domain.Identity.Enums.UserRole.Family;

            user = User.Create(new CreateUserArg(id, command.PhoneNumber, role));
            await userRepo.AddAsync(user, token);
        }
        else if (user.Status == Domain.Identity.Enums.UserStatus.Suspended)
        {
            throw new UserSuspendedException();
        }

        user.MarkLoggedIn();
        var result = await IssueTokensAsync(user, token);
        await db.SaveChangesAsync(token);
        return result;
    }

    public async Task<LoginResponse> Handle(RefreshTokenCommand command, CancellationToken token)
    {
        var storedToken = await refreshTokenRepo.GetByTokenAsync(command.RefreshToken, token)
            ?? throw new InvalidRefreshTokenException();

        if (!storedToken.IsValid())
            throw new InvalidRefreshTokenException();

        var user = await userRepo.GetByIdAsync(storedToken.UserId, token)
            ?? throw new UserNotFoundException();

        storedToken.Revoke();
        await refreshTokenRepo.UpdateAsync(storedToken, token);

        var result = await IssueTokensAsync(user, token);
        await db.SaveChangesAsync(token);
        return result;
    }

    public async Task<bool> Handle(LogoutCommand command, CancellationToken token)
    {
        var storedToken = await refreshTokenRepo.GetByTokenAsync(command.RefreshToken, token);
        if (storedToken is null) return true;

        storedToken.Revoke();
        await refreshTokenRepo.UpdateAsync(storedToken, token);
        await db.SaveChangesAsync(token);
        return true;
    }

    public async Task<bool> Handle(UpdateProfileCommand command, CancellationToken token)
    {
        var user = await userRepo.GetByIdAsync(command.UserId, token)
            ?? throw new UserNotFoundException();

        user.UpdateProfile(command.FirstName, command.LastName, command.NationalId);
        await userRepo.UpdateAsync(user, token);
        await db.SaveChangesAsync(token);
        return true;
    }

    /// <summary>
    /// نقش ادمین فقط از روی شماره‌های اعلام‌شده در تنظیمات داده می‌شود، نه ورودی کلاینت.
    /// </summary>
    private bool IsPlatformAdmin(string phoneNumber)
    {
        var adminPhones = configuration.GetSection("Platform:AdminPhones").Get<string[]>() ?? [];
        return adminPhones.Contains(phoneNumber);
    }

    private async Task<LoginResponse> IssueTokensAsync(User user, CancellationToken ct)
    {
        var accessToken  = jwtService.GenerateAccessToken(user);
        var refreshValue = jwtService.GenerateRefreshToken();
        var id           = await refreshTokenRepo.GetNextIdAsync();
        var refreshToken = new RefreshToken(id, user.Id, refreshValue, DateTime.UtcNow.AddDays(30));

        await refreshTokenRepo.AddAsync(refreshToken, ct);
        return new LoginResponse(accessToken, refreshValue, user.Id, user.Role.ToString(), user.DisplayName);
    }
}
