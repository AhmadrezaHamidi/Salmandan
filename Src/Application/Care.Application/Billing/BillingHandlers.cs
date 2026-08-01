using Care.Domain.Agency.Exceptions;
using Care.Domain.Billing.Aggregates;
using Care.Domain.Billing.Args;
using Care.Domain.Billing.Exceptions;
using Care.Domain.Identity.Exceptions;
using Care.Domain.Senior.Exceptions;
using Care.Domain.Shift.Exceptions;
using Microsoft.Extensions.Configuration;

namespace Care.Application.Billing;

/// <summary>
/// Handler های پرداخت امانی.
/// وجه خانواده تا تأیید گزارش شیفت نزد پلتفرم می‌ماند — ستون فقرات اعتماد دوطرفه.
/// </summary>
public sealed class BillingHandlers(
    ICareOrderRepository orderRepo,
    IShiftRepository     shiftRepo,
    ISeniorRepository    seniorRepo,
    IAgencyRepository    agencyRepo,
    IPaymentGateway      paymentGateway,
    IConfiguration       configuration,
    CareDbContext        db) :
    ICommandHandler<CreateCareOrderCommand, CreateOrderResponse>,
    ICommandHandler<ConfirmPaymentCommand, bool>,
    ICommandHandler<ReleaseOrderCommand, bool>,
    ICommandHandler<RefundOrderCommand, bool>,
    ICommandHandler<IssueInvoiceCommand, string>
{
    public async Task<CreateOrderResponse> Handle(CreateCareOrderCommand command, CancellationToken token)
    {
        var shift = await shiftRepo.GetByIdAsync(command.ShiftId, token)
            ?? throw new ShiftNotFoundException();

        var senior = await seniorRepo.GetWithFamilyAsync(shift.SeniorId, token)
            ?? throw new SeniorNotFoundException();

        if (!senior.CanBookShift(command.PayerUserId)) throw new AccessDeniedException();

        // برای هر شیفت فقط یک سفارش؛ اگر قبلاً ساخته شده همان برگردانده می‌شود
        var existing = await orderRepo.GetByShiftIdAsync(command.ShiftId, token);
        if (existing is not null && existing.Status == Domain.Billing.Enums.OrderStatus.Pending)
            return await RequestPaymentAsync(existing, token);

        if (existing is not null)
            throw new OrderAlreadyEscrowedException();

        var agency = await agencyRepo.GetByIdAsync(shift.AgencyId, token)
            ?? throw new AgencyNotFoundException();

        var id    = await orderRepo.GetNextIdAsync();
        var order = CareOrder.Create(new CreateOrderArg(
            id, shift.Id, shift.SeniorId, shift.AgencyId,
            command.PayerUserId, shift.Price, agency.CommissionPercent));

        await orderRepo.AddAsync(order, token);
        await db.SaveChangesAsync(token);

        return await RequestPaymentAsync(order, token);
    }

    public async Task<bool> Handle(ConfirmPaymentCommand command, CancellationToken token)
    {
        var order = await orderRepo.GetByAuthorityAsync(command.Authority, token)
            ?? throw new OrderNotFoundException();

        var verify = await paymentGateway.VerifyPaymentAsync(command.Authority, order.Amount, token);
        if (!verify.Success)
        {
            order.MarkFailed();
            await orderRepo.UpdateAsync(order, token);
            await db.SaveChangesAsync(token);
            throw new PaymentFailedException();
        }

        order.MarkEscrowed(verify.RefId, command.Authority);
        await orderRepo.UpdateAsync(order, token);
        await db.SaveChangesAsync(token);
        return true;
    }

    public async Task<bool> Handle(ReleaseOrderCommand command, CancellationToken token)
    {
        var order = await GetOrderAsync(command.OrderId, token);
        order.Release();
        await orderRepo.UpdateAsync(order, token);
        await db.SaveChangesAsync(token);
        return true;
    }

    public async Task<bool> Handle(RefundOrderCommand command, CancellationToken token)
    {
        var order = await GetOrderAsync(command.OrderId, token);
        order.Refund(command.Reason);
        await orderRepo.UpdateAsync(order, token);
        await db.SaveChangesAsync(token);
        return true;
    }

    /// <summary>
    /// صدور فاکتور رسمی — اهرم اصلی رقابت با بازار غیررسمی،
    /// چون پرستار غیررسمی نمی‌تواند فاکتور بیمه‌پذیر بدهد.
    /// </summary>
    public async Task<string> Handle(IssueInvoiceCommand command, CancellationToken token)
    {
        var order = await GetOrderAsync(command.OrderId, token);

        var invoiceNumber = $"SLM-{DateTime.UtcNow:yyyyMM}-{order.Id:D8}";
        order.IssueInvoice(invoiceNumber);

        await orderRepo.UpdateAsync(order, token);
        await db.SaveChangesAsync(token);
        return invoiceNumber;
    }

    private async Task<CreateOrderResponse> RequestPaymentAsync(CareOrder order, CancellationToken ct)
    {
        var callbackUrl = configuration["PaymentGateway:CallbackUrl"] ?? "https://localhost/api/v1/Orders/Callback";
        var payment     = await paymentGateway.RequestPaymentAsync(
            order.Amount, $"پرداخت شیفت مراقبت #{order.ShiftId}", callbackUrl, ct);

        // وضعیت تا تأیید درگاه Pending می‌ماند؛ فقط شناسه تراکنش ذخیره می‌شود
        order.AttachPaymentAuthority(payment.Authority);
        await orderRepo.UpdateAsync(order, ct);
        await db.SaveChangesAsync(ct);

        return new CreateOrderResponse(order.Id, order.Amount, payment.PaymentUrl, payment.Authority);
    }

    private async Task<CareOrder> GetOrderAsync(long id, CancellationToken ct)
        => await orderRepo.GetByIdAsync(id, ct) ?? throw new OrderNotFoundException();
}
