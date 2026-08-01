using AhmadBase.External.Service.Payment;
using Care.Application.Contract.Services;
using Microsoft.Extensions.Logging;

namespace Care.Persistence.EF.Services;

/// <summary>
/// Adapter بین IPaymentGateway (قرارداد لایه Application) و ZarinPalService (فریم‌ورک).
/// Circuit Breaker داخل ZarinPalService پیاده‌سازی شده است.
/// </summary>
public sealed class ZarinPalPaymentGateway(
    IPaymentService zarinpal,
    ILogger<ZarinPalPaymentGateway>? logger = null) : IPaymentGateway
{
    public async Task<Care.Application.Contract.Services.PaymentRequestResult> RequestPaymentAsync(
        decimal           amount,
        string            description,
        string            callbackUrl,
        CancellationToken ct = default)
    {
        var result = await zarinpal.RequestAsync(new PaymentRequest(amount, description, callbackUrl), ct);

        if (!result.Success)
        {
            logger?.LogWarning("ZarinPal request failed: {Error}", result.Error);
            throw new InvalidOperationException(result.Error ?? "خطا در اتصال به درگاه پرداخت");
        }

        return new Care.Application.Contract.Services.PaymentRequestResult(result.Authority!, result.RedirectUrl!);
    }

    public async Task<Care.Application.Contract.Services.PaymentVerifyResult> VerifyPaymentAsync(
        string            authority,
        decimal           amount,
        CancellationToken ct = default)
    {
        var result = await zarinpal.VerifyAsync(new PaymentVerifyRequest(authority, amount), ct);

        return new Care.Application.Contract.Services.PaymentVerifyResult(
            result.Success, result.TransactionCode ?? string.Empty);
    }
}
