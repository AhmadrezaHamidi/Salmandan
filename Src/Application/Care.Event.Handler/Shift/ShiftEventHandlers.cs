using Care.Application.Contract.Services;
using Care.Domain.Events.Shift;

namespace Care.Event.Handler.Shift;

/// <summary>ثبت درخواست شیفت — اطلاع به مؤسسه برای تخصیص پرستار</summary>
public sealed class ShiftRequestedEventHandler(ILogger<ShiftRequestedEventHandler> logger)
    : IEventHandlerAsync<ShiftRequestedEvent>
{
    public Task HandleAsync(ShiftRequestedEvent @event, CancellationToken ct)
    {
        logger.LogInformation(
            "شیفت {ShiftId} برای سالمند {SeniorId} در مؤسسه {AgencyId} تاریخ {Date} ساعت {StartTime} ثبت شد",
            @event.ShiftId, @event.SeniorId, @event.AgencyId, @event.Date, @event.StartTime);
        // TODO: اعلان به پنل مؤسسه برای تخصیص پرستار
        return Task.CompletedTask;
    }
}

/// <summary>تخصیص پرستار — اطلاع به خانواده</summary>
public sealed class ShiftAssignedEventHandler(
    IFamilyNotifier notifier,
    ILogger<ShiftAssignedEventHandler> logger)
    : IEventHandlerAsync<ShiftAssignedEvent>
{
    public async Task HandleAsync(ShiftAssignedEvent @event, CancellationToken ct)
    {
        logger.LogInformation("پرستار {CaregiverId} به شیفت {ShiftId} تخصیص یافت",
            @event.CaregiverId, @event.ShiftId);

        await notifier.NotifyFamilyAsync(@event.SeniorId, NotificationTemplates.CaregiverAssigned, ct);
    }
}

/// <summary>چک‌این پرستار — پیامک «پرستار رسید» به خانواده</summary>
public sealed class ShiftCheckedInEventHandler(
    IFamilyNotifier notifier,
    ILogger<ShiftCheckedInEventHandler> logger)
    : IEventHandlerAsync<ShiftCheckedInEvent>
{
    public async Task HandleAsync(ShiftCheckedInEvent @event, CancellationToken ct)
    {
        logger.LogInformation("پرستار {CaregiverId} در شیفت {ShiftId} ساعت {CheckInAt} حاضر شد",
            @event.CaregiverId, @event.ShiftId, @event.CheckInAt);

        await notifier.NotifyFamilyAsync(@event.SeniorId, NotificationTemplates.CaregiverArrived(@event.CheckInAt), ct);
    }
}

/// <summary>تکمیل شیفت — تریگر آزادسازی وجه از امانت</summary>
public sealed class ShiftCompletedEventHandler(ILogger<ShiftCompletedEventHandler> logger)
    : IEventHandlerAsync<ShiftCompletedEvent>
{
    public Task HandleAsync(ShiftCompletedEvent @event, CancellationToken ct)
    {
        logger.LogInformation("شیفت {ShiftId} تکمیل شد — آماده آزادسازی وجه به مؤسسه {AgencyId}",
            @event.ShiftId, @event.AgencyId);
        // TODO: فراخوانی ReleaseOrderCommand
        return Task.CompletedTask;
    }
}

/// <summary>
/// عدم حضور پرستار — بحرانی‌ترین رویداد سیستم.
/// آغاز فرآیند تضمین پوشش شیفت: یافتن جایگزین ظرف مهلت مقرر.
/// </summary>
public sealed class ShiftNoShowEventHandler(ILogger<ShiftNoShowEventHandler> logger)
    : IEventHandlerAsync<ShiftNoShowEvent>
{
    public Task HandleAsync(ShiftNoShowEvent @event, CancellationToken ct)
    {
        logger.LogWarning(
            "عدم حضور در شیفت {ShiftId} سالمند {SeniorId} مؤسسه {AgencyId} — فرآیند جایگزینی آغاز شد",
            @event.ShiftId, @event.SeniorId, @event.AgencyId);
        // TODO: اعلان اضطراری به مؤسسه و خانواده، شروع تایمر SLA
        return Task.CompletedTask;
    }
}

/// <summary>لغو شیفت — اطلاع به طرف مقابل و بررسی عودت وجه</summary>
public sealed class ShiftCancelledEventHandler(ILogger<ShiftCancelledEventHandler> logger)
    : IEventHandlerAsync<ShiftCancelledEvent>
{
    public Task HandleAsync(ShiftCancelledEvent @event, CancellationToken ct)
    {
        logger.LogInformation("شیفت {ShiftId} لغو شد. دلیل: {Reason}", @event.ShiftId, @event.Reason);
        return Task.CompletedTask;
    }
}

/// <summary>شیفت جایگزین ساخته شد — وعده پوشش اجرا شد</summary>
public sealed class ShiftReplacedEventHandler(ILogger<ShiftReplacedEventHandler> logger)
    : IEventHandlerAsync<ShiftReplacedEvent>
{
    public Task HandleAsync(ShiftReplacedEvent @event, CancellationToken ct)
    {
        logger.LogInformation("شیفت {OriginalShiftId} با شیفت {ReplacementShiftId} جایگزین شد",
            @event.OriginalShiftId, @event.ReplacementShiftId);
        // TODO: اعلان به خانواده — «جایگزین اعزام شد»
        return Task.CompletedTask;
    }
}
