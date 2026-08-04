using Care.Application.Contract.Services;
using Care.Domain.Events.Report;

namespace Care.Event.Handler.Report;

/// <summary>
/// گزارش پایان شیفت ثبت شد — خانواده باخبر می‌شود.
/// این پیامک، تفاوت روزمره‌ی مؤسسه‌ی ما با مؤسسه‌ی سنتی است.
/// </summary>
public sealed class ShiftReportSubmittedEventHandler(
    IFamilyNotifier notifier,
    ILogger<ShiftReportSubmittedEventHandler> logger)
    : IEventHandlerAsync<ShiftReportSubmittedEvent>
{
    public async Task HandleAsync(ShiftReportSubmittedEvent @event, CancellationToken ct)
    {
        logger.LogInformation("گزارش {ReportId} برای شیفت {ShiftId} ثبت شد", @event.ReportId, @event.ShiftId);
        await notifier.NotifyFamilyAsync(@event.SeniorId, NotificationTemplates.ReportReady, ct);
    }
}

/// <summary>حادثه در شیفت — اطلاع فوری به کل حلقه خانواده</summary>
public sealed class IncidentReportedEventHandler(
    IFamilyNotifier notifier,
    ILogger<IncidentReportedEventHandler> logger)
    : IEventHandlerAsync<IncidentReportedEvent>
{
    public async Task HandleAsync(IncidentReportedEvent @event, CancellationToken ct)
    {
        logger.LogWarning("حادثه {IncidentType} برای سالمند {SeniorId}: {Description}",
            @event.IncidentType, @event.SeniorId, @event.Description);

        await notifier.NotifyFamilyAsync(@event.SeniorId, NotificationTemplates.Incident(@event.Description), ct);
    }
}

/// <summary>تشخیص روند نزولی سلامت — فعلاً فقط لاگ، تا مدل به‌اندازه کافی داده ببیند</summary>
public sealed class HealthDeteriorationDetectedEventHandler(
    ILogger<HealthDeteriorationDetectedEventHandler> logger)
    : IEventHandlerAsync<HealthDeteriorationDetectedEvent>
{
    public Task HandleAsync(HealthDeteriorationDetectedEvent @event, CancellationToken ct)
    {
        logger.LogWarning("هشدار روند سلامت سالمند {SeniorId} — امتیاز {RiskScore} — {RiskFactor}",
            @event.SeniorId, @event.RiskScore, @event.RiskFactor);
        return Task.CompletedTask;
    }
}
