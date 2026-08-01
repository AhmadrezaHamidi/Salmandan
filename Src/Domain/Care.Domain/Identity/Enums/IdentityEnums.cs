namespace Care.Domain.Identity.Enums;

/// <summary>نقش کاربر در پلتفرم</summary>
public enum UserRole
{
    /// <summary>ادمین پلتفرم</summary>
    Admin = 1,

    /// <summary>عضو خانواده سالمند (مشتری اصلی)</summary>
    Family = 2,

    /// <summary>مدیر مؤسسه پرستاری</summary>
    AgencyManager = 3,

    /// <summary>پرستار / مراقب</summary>
    Caregiver = 4
}

/// <summary>وضعیت کاربر</summary>
public enum UserStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3
}
