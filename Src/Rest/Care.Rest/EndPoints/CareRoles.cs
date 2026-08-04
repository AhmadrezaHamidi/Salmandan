using Microsoft.AspNetCore.Authorization;

namespace Care.Rest.EndPoints;

/// <summary>
/// نقش‌های پلتفرم — مقادیر دقیقاً برابر نام‌های <c>UserRole</c> در دامین است،
/// چون JwtService نقش را با <c>role.ToString()</c> داخل توکن می‌گذارد.
/// </summary>
public static class CareRoles
{
    public const string Admin         = "Admin";
    public const string Family        = "Family";
    public const string AgencyManager = "AgencyManager";
    public const string Caregiver     = "Caregiver";

    /// <summary>فقط ادمین پلتفرم — تأیید مؤسسه، تعلیق، تعیین کمیسیون</summary>
    public const string AdminOnly = Admin;

    /// <summary>ادمین یا مدیر مؤسسه — عملیات داخلی مؤسسه</summary>
    public const string AgencyStaff = Admin + "," + AgencyManager;

    /// <summary>ادمین یا خانواده — پرونده سالمند و رزرو شیفت</summary>
    public const string FamilyOrAdmin = Admin + "," + Family;

    /// <summary>ادمین یا پرستار — چک‌این/چک‌اوت و ثبت گزارش</summary>
    public const string CaregiverOrAdmin = Admin + "," + Caregiver;

    /// <summary>خانواده یا مؤسسه — عملیاتی که هر دو طرف می‌توانند انجام دهند مثل لغو شیفت</summary>
    public const string FamilyOrAgency = Admin + "," + Family + "," + AgencyManager;
}

public static class ClaimsPrincipalExtensions
{
    /// <summary>نقش کاربر جاری از توکن — ورودی کنترل دسترسی سطح رکورد</summary>
    public static string? RoleOf(this System.Security.Claims.ClaimsPrincipal principal)
        => principal.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
}

public static class EndpointRoleExtensions
{
    /// <summary>محدود کردن endpoint به نقش‌های مشخص</summary>
    public static TBuilder RequireRoles<TBuilder>(this TBuilder builder, string roles)
        where TBuilder : IEndpointConventionBuilder
        => builder.RequireAuthorization(new AuthorizeAttribute { Roles = roles });
}
