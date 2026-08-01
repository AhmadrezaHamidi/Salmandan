using Care.Application.Contract.Services;
using Care.Domain.Report.Enums;
using Care.Domain.Report.Repositories;

namespace Care.Persistence.EF.Services;

/// <summary>
/// تحلیل روند سلامت سالمند از روی گزارش‌های اخیر شیفت.
/// نسخه قاعده‌محور — الگوی Cera: تشخیص روند نزولی از داده‌ی ساختاریافته‌ی مراقب،
/// بدون نیاز به wearable یا سخت‌افزار.
/// جایگزینی با مدل یادگیری ماشین در فاز بعد، بدون تغییر این قرارداد ممکن است.
/// </summary>
public sealed class HealthRiskAnalyzer(IShiftReportRepository reportRepo) : IHealthRiskAnalyzer
{
    /// <summary>تعداد گزارش اخیری که مبنای تحلیل قرار می‌گیرد</summary>
    private const int WindowSize = 7;

    public async Task<HealthRiskResult> AnalyzeAsync(long seniorId, CancellationToken ct = default)
    {
        var reports = await reportRepo.GetRecentBySeniorAsync(seniorId, WindowSize, ct);

        if (reports.Count == 0)
            return new HealthRiskResult(seniorId, 0, "Unknown", [], "هنوز گزارشی برای تحلیل ثبت نشده است.");

        // گزارش‌ها از جدید به قدیم مرتب‌اند
        var factors  = new List<string>();
        var avgScore = (int)Math.Round(reports.Average(r => r.RiskScore));

        // ── روند اشتها ──
        if (reports.Count >= 3 && reports.Take(3).All(r => r.Appetite == ObservationScale.Poor))
            factors.Add("کاهش اشتها در ۳ شیفت متوالی");

        // ── روند خواب ──
        if (reports.Count >= 3 && reports.Take(3).All(r => r.SleepQuality == ObservationScale.Poor))
            factors.Add("بی‌کیفیتی خواب در ۳ شیفت متوالی");

        // ── روند خلق ──
        if (reports.Count >= 3 && reports.Take(3).All(r => r.Mood == ObservationScale.Poor))
            factors.Add("افت خلق در ۳ شیفت متوالی");

        // ── پایبندی به دارو ──
        var adherence = reports.Average(r => r.MedicationAdherence);
        if (adherence < 0.8m)
            factors.Add($"پایبندی دارویی پایین ({adherence:P0})");

        // ── درد ──
        if (reports.Take(3).Any(r => r.PainLevel >= 7))
            factors.Add("گزارش درد شدید در شیفت‌های اخیر");

        // ── حادثه ──
        var incidents = reports.Count(r => r.HasIncident);
        if (incidents > 0)
            factors.Add($"{incidents} حادثه ثبت‌شده در {reports.Count} شیفت اخیر");

        // ── افت تحرک ──
        if (reports.Count >= 4)
        {
            var recentMobility = reports.Take(2).Average(r => (int)r.Mobility);
            var olderMobility  = reports.Skip(2).Average(r => (int)r.Mobility);
            if (recentMobility < olderMobility - 0.5)
                factors.Add("کاهش سطح تحرک نسبت به شیفت‌های قبل");
        }

        // ── فشار خون ناپایدار ──
        var bpReadings = reports.Where(r => r.BloodPressureSystolic.HasValue).ToList();
        if (bpReadings.Count >= 3 && bpReadings.Count(r => r.BloodPressureSystolic is > 160 or < 90) >= 2)
            factors.Add("فشار خون خارج از محدوده نرمال در چند نوبت");

        var riskLevel = avgScore switch
        {
            >= 60 => "High",
            >= 30 => "Medium",
            _     => "Low"
        };

        return new HealthRiskResult(seniorId, avgScore, riskLevel, factors, BuildSummary(riskLevel, factors));
    }

    /// <summary>خلاصه فارسی قابل نمایش به خانواده — نه لاگ خام، یک جمله‌ی قابل فهم</summary>
    private static string BuildSummary(string riskLevel, List<string> factors)
    {
        if (factors.Count == 0)
            return "وضعیت در شیفت‌های اخیر پایدار بوده و نکته نگران‌کننده‌ای ثبت نشده است.";

        var prefix = riskLevel switch
        {
            "High"   => "نیاز به پیگیری فوری:",
            "Medium" => "چند نکته قابل توجه:",
            _        => "نکات جزئی:"
        };

        return $"{prefix} {string.Join(' ', factors.Select(f => $"{f}."))}";
    }
}
