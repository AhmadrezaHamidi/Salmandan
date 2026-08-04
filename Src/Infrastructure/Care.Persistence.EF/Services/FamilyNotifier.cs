using AhmadBase.External.Service;
using Care.Application.Contract.Services;
using Microsoft.Extensions.Logging;

namespace Care.Persistence.EF.Services;

/// <summary>
/// ارسال پیامک به اعضای حلقه خانواده.
/// خطای ارسال هرگز عملیات مراقبت را نمی‌شکند — فقط لاگ می‌شود.
/// </summary>
public sealed class FamilyNotifier(
    CareDbContext db,
    ISmsService   sms,
    ILogger<FamilyNotifier> logger) : IFamilyNotifier
{
    public async Task NotifyFamilyAsync(long seniorId, string message, CancellationToken ct = default)
    {
        var phones = await db.FamilyMembers
            .Where(f => f.SeniorId == seniorId)
            .Join(db.Users, f => f.UserId, u => u.Id, (_, u) => u.PhoneNumber)
            .Distinct()
            .ToListAsync(ct);

        if (phones.Count == 0)
        {
            logger.LogWarning("سالمند {SeniorId} هیچ عضو خانواده‌ای برای اطلاع‌رسانی ندارد", seniorId);
            return;
        }

        await SendAsync(phones, message, ct);
    }

    public Task NotifyAsync(string phoneNumber, string message, CancellationToken ct = default)
        => SendAsync([phoneNumber], message, ct);

    private async Task SendAsync(IReadOnlyList<string> phones, string message, CancellationToken ct)
    {
        try
        {
            await sms.SendBulkAsync(phones, message, ct);
            logger.LogInformation("پیامک به {Count} شماره ارسال شد", phones.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ارسال پیامک ناموفق بود — {Count} گیرنده", phones.Count);
        }
    }
}
