namespace Identity.Application.Commands;

/// <summary>درخواست ارسال کد یکبار مصرف</summary>
public record RequestOtpCommand(string PhoneNumber) : ICommand<RequestOtpResponse>;

/// <summary>
/// تأیید کد و ورود — کاربر جدید خودکار با نقش «خانواده» ساخته می‌شود.
///
/// نقش عمداً ورودی نمی‌گیرد: اگر کلاینت نقش خودش را تعیین کند، هر کسی می‌تواند
/// خودش را Admin اعلام کند و کل کنترل دسترسی بی‌معنی می‌شود.
/// نقش پرستار و مدیر مؤسسه هنگام ثبت آن‌ها توسط سیستم داده می‌شود.
/// </summary>
public record VerifyOtpCommand(string PhoneNumber, string Code) : ICommand<LoginResponse>;

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
