using Care.Domain.Identity.Enums;

namespace Identity.Application.Commands;

/// <summary>درخواست ارسال کد یکبار مصرف</summary>
public record RequestOtpCommand(string PhoneNumber) : ICommand<RequestOtpResponse>;

/// <summary>تأیید کد و ورود — کاربر جدید خودکار ساخته می‌شود</summary>
public record VerifyOtpCommand(string PhoneNumber, string Code, UserRole Role = UserRole.Family) : ICommand<LoginResponse>;

/// <summary>تمدید توکن دسترسی</summary>
public record RefreshTokenCommand(string RefreshToken) : ICommand<LoginResponse>;

/// <summary>خروج و ابطال توکن تمدید</summary>
public record LogoutCommand(string RefreshToken) : ICommand<bool>;

/// <summary>تکمیل یا بروزرسانی پروفایل کاربر</summary>
public record UpdateProfileCommand(
    [property: JsonIgnore] long UserId,
    string? FirstName,
    string? LastName,
    string? NationalId) : ICommand<bool>;

public sealed record RequestOtpResponse(string Message);

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    long   UserId,
    string Role,
    string DisplayName);
