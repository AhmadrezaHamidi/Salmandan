using Care.Domain.Identity.Args;
using Care.Domain.Identity.Enums;

namespace Care.Domain.Identity.Aggregates;

/// <summary>
/// Aggregate Root کاربر پلتفرم سالمندان
/// شامل عضو خانواده، مدیر مؤسسه، پرستار و ادمین
/// </summary>
public sealed class User : AggregateRoot<long>
{
    public string     PhoneNumber { get; private set; } = string.Empty;
    public string?    FirstName   { get; private set; }
    public string?    LastName    { get; private set; }
    public string?    NationalId  { get; private set; }
    public UserRole   Role        { get; private set; }
    public UserStatus Status      { get; private set; }
    public DateTime   CreatedAt   { get; private set; }
    public DateTime?  LastLoginAt { get; private set; }

    /// <summary>نام کامل نمایشی — اگر پروفایل تکمیل نشده باشد شماره موبایل</summary>
    public string DisplayName =>
        string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName)
            ? PhoneNumber
            : $"{FirstName} {LastName}".Trim();

    private User() { }

    private User(CreateUserArg arg) : base(arg.Id)
    {
        PhoneNumber = arg.PhoneNumber;
        Role        = arg.Role;
        Status      = UserStatus.Active;
        CreatedAt   = DateTime.UtcNow;
    }

    /// <summary>ایجاد کاربر جدید هنگام اولین ورود با OTP</summary>
    public static User Create(CreateUserArg arg)
    {
        var user = new User(arg);
        user.RaiseDomainEvent(new UserRegisteredEvent(arg.Id, arg.PhoneNumber, arg.Role.ToString()));
        return user;
    }

    /// <summary>بروزرسانی اطلاعات پروفایل</summary>
    public void UpdateProfile(string? firstName, string? lastName, string? nationalId)
    {
        FirstName  = firstName;
        LastName   = lastName;
        NationalId = nationalId;
    }

    /// <summary>ثبت زمان آخرین ورود موفق</summary>
    public void MarkLoggedIn()
    {
        LastLoginAt = DateTime.UtcNow;
        RaiseDomainEvent(new UserLoggedInEvent(Id, PhoneNumber));
    }

    public void Activate()   => Status = UserStatus.Active;
    public void Deactivate() => Status = UserStatus.Inactive;
    public void Suspend()    => Status = UserStatus.Suspended;

    /// <summary>تغییر نقش کاربر — مثلاً وقتی عضو خانواده مدیر مؤسسه هم می‌شود</summary>
    public void SetRole(UserRole role) => Role = role;
}
