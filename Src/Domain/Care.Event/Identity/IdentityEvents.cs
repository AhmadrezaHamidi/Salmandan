using AhmadBase.Doamin;

namespace Care.Domain.Events.Identity;

/// <summary>رویداد ثبت‌نام کاربر جدید</summary>
public sealed record UserRegisteredEvent(long UserId, string PhoneNumber, string Role) : IEvent;

/// <summary>رویداد ورود موفق کاربر</summary>
public sealed record UserLoggedInEvent(long UserId, string PhoneNumber) : IEvent;

/// <summary>رویداد درخواست کد یکبار مصرف</summary>
public sealed record OtpRequestedEvent(string PhoneNumber, string Code) : IEvent;
