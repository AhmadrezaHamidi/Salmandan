using Care.Domain.Agency.Exceptions;
using Care.Domain.Payroll.Aggregates;
using Care.Domain.Payroll.Args;
using Care.Domain.Payroll.Exceptions;

namespace Care.Application.Payroll;

/// <summary>
/// Handler های حقوق و دستمزد پرستار.
///
/// مبنای محاسبه، شیفت‌های تکمیل‌شده است — نه دفتر دستی. چون چک‌این/چک‌اوت ژئوفنسی
/// داریم، هم مؤسسه و هم پرستار به یک عدد نگاه می‌کنند و جای بحث نمی‌ماند.
/// </summary>
public sealed class PayrollHandlers(
    IPayoutRepository payoutRepo,
    IShiftRepository  shiftRepo,
    IAgencyRepository agencyRepo,
    CareDbContext     db) :
    ICommandHandler<GeneratePayrollCommand, GeneratePayrollResponse>,
    ICommandHandler<MarkPayoutPaidCommand, bool>,
    ICommandHandler<CancelPayoutCommand, bool>
{
    public async Task<GeneratePayrollResponse> Handle(GeneratePayrollCommand command, CancellationToken token)
    {
        if (command.To < command.From) throw new InvalidPayoutPeriodException();

        _ = await agencyRepo.GetByIdAsync(command.AgencyId, token)
            ?? throw new AgencyNotFoundException();

        var shifts = await shiftRepo.GetUnpaidCompletedAsync(command.AgencyId, command.From, command.To, token);
        if (shifts.Count == 0) throw new NoShiftsToPayException();

        var payoutIds   = new List<long>();
        var totalAmount = 0m;

        // یک فیش به‌ازای هر پرستار
        foreach (var group in shifts.Where(s => s.CaregiverId.HasValue).GroupBy(s => s.CaregiverId!.Value))
        {
            var caregiverShifts = group.ToList();
            var amount = caregiverShifts.Sum(s => s.CaregiverPayRate);
            var hours  = (decimal)caregiverShifts.Sum(s => s.WorkedHours);

            var payoutId = await payoutRepo.GetNextIdAsync();
            var payout   = CaregiverPayout.Create(new CreatePayoutArg(
                payoutId, command.AgencyId, group.Key, command.From, command.To,
                caregiverShifts.Count, Math.Round(hours, 2), amount));

            // قفل کردن شیفت‌ها به این فیش تا در دوره بعد دوباره حساب نشوند
            foreach (var shift in caregiverShifts)
            {
                shift.IncludeInPayout(payoutId);
                await shiftRepo.UpdateAsync(shift, token);
            }

            await payoutRepo.AddAsync(payout, token);
            payoutIds.Add(payoutId);
            totalAmount += amount;
        }

        await db.SaveChangesAsync(token);

        return new GeneratePayrollResponse(payoutIds.Count, shifts.Count, totalAmount, payoutIds);
    }

    public async Task<bool> Handle(MarkPayoutPaidCommand command, CancellationToken token)
    {
        var payout = await payoutRepo.GetByIdAsync(command.PayoutId, token)
            ?? throw new PayoutNotFoundException();

        payout.MarkPaid(command.PaymentReference);
        await payoutRepo.UpdateAsync(payout, token);
        await db.SaveChangesAsync(token);
        return true;
    }

    public async Task<bool> Handle(CancelPayoutCommand command, CancellationToken token)
    {
        var payout = await payoutRepo.GetByIdAsync(command.PayoutId, token)
            ?? throw new PayoutNotFoundException();

        payout.Cancel(command.Reason);

        // شیفت‌ها آزاد می‌شوند تا در فیش بعدی دوباره حساب شوند
        var shifts = await shiftRepo.GetByPayoutAsync(command.PayoutId, token);
        foreach (var shift in shifts)
        {
            shift.ReleaseFromPayout();
            await shiftRepo.UpdateAsync(shift, token);
        }

        await payoutRepo.UpdateAsync(payout, token);
        await db.SaveChangesAsync(token);
        return true;
    }
}
