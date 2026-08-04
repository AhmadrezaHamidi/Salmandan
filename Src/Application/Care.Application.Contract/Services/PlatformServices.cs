using Care.Domain.Identity.Aggregates;

namespace Care.Application.Contract.Services;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    (long userId, string role) ValidateRefreshToken(string token);
}

public sealed record PaymentRequestResult(string Authority, string PaymentUrl);
public sealed record PaymentVerifyResult(bool Success, string RefId);

public interface IPaymentGateway
{
    Task<PaymentRequestResult> RequestPaymentAsync(decimal amount, string description, string callbackUrl, CancellationToken ct = default);
    Task<PaymentVerifyResult>  VerifyPaymentAsync(string authority, decimal amount, CancellationToken ct = default);
}

public interface IPaymentSettings
{
    string CallbackUrl { get; }
}

/// <summary>
/// اطلاع‌رسانی به اعضای حلقه خانواده یک سالمند.
/// همان چیزی که «آرامش خاطر» را از یک وعده به یک تجربه‌ی روزمره تبدیل می‌کند.
/// </summary>
public interface IFamilyNotifier
{
    /// <summary>ارسال پیام به همه‌ی اعضای حلقه خانواده</summary>
    Task NotifyFamilyAsync(long seniorId, string message, CancellationToken ct = default);

    /// <summary>ارسال پیام به یک شماره مشخص</summary>
    Task NotifyAsync(string phoneNumber, string message, CancellationToken ct = default);
}

/// <summary>
/// تحلیل روند سلامت سالمند از روی گزارش‌های شیفت.
/// نسخه اول قاعده‌محور است؛ در فاز بعد جای آن مدل یادگیری ماشین می‌نشیند.
/// </summary>
public interface IHealthRiskAnalyzer
{
    /// <summary>محاسبه امتیاز ریسک و عوامل مؤثر بر مبنای N گزارش اخیر</summary>
    Task<HealthRiskResult> AnalyzeAsync(long seniorId, CancellationToken ct = default);
}

/// <summary>خروجی تحلیل ریسک — امتیاز، سطح و عوامل قابل ارائه به خانواده</summary>
public sealed record HealthRiskResult(
    long                 SeniorId,
    int                  RiskScore,
    string               RiskLevel,
    IReadOnlyList<string> RiskFactors,
    string               Summary);
