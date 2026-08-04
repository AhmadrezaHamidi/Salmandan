using Care.Domain.Agency.Aggregates;
using Care.Domain.Agency.Args;
using Care.Domain.Agency.Exceptions;

namespace Care.Application.Agency;

/// <summary>Handler های مدیریت مؤسسات پرستاری — طرف عرضه پلتفرم</summary>
public sealed class AgencyHandlers(
    IAgencyRepository agencyRepo,
    IUserRepository   userRepo,
    CareDbContext     db) :
    ICommandHandler<RegisterAgencyCommand, long>,
    ICommandHandler<ApproveAgencyCommand, bool>,
    ICommandHandler<RejectAgencyCommand, bool>,
    ICommandHandler<SuspendAgencyCommand, bool>,
    ICommandHandler<RenewAgencyLicenseCommand, bool>,
    ICommandHandler<UpdateAgencyProfileCommand, bool>,
    ICommandHandler<SetAgencyCommissionCommand, bool>,
    ICommandHandler<SetAgencyServicePriceCommand, bool>
{
    /// <summary>کمیسیون پیش‌فرض پلتفرم — عمداً نازک نگه داشته شده؛ درآمد اصلی از اشتراک و SaaS است</summary>
    private const decimal DefaultCommissionPercent = 12m;

    public async Task<long> Handle(RegisterAgencyCommand command, CancellationToken token)
    {
        if (await agencyRepo.LicenseNumberExistsAsync(command.LicenseNumber, token))
            throw new DuplicateLicenseNumberException();

        var owner = await userRepo.GetByIdAsync(command.OwnerUserId, token)
            ?? throw new Domain.Identity.Exceptions.UserNotFoundException();

        // مالک مؤسسه به نقش مدیر مؤسسه ارتقا می‌یابد
        // توجه: توکن فعلی او هنوز نقش قبلی را دارد و باید دوباره وارد شود
        owner.PromoteTo(Domain.Identity.Enums.UserRole.AgencyManager);
        await userRepo.UpdateAsync(owner, token);

        var id     = await agencyRepo.GetNextIdAsync();
        var agency = CareAgency.Register(new CreateAgencyArg(
            id, command.Name, command.LicenseNumber, command.LicenseExpiryDate,
            command.OwnerUserId, command.City, command.District, command.Address,
            command.PhoneNumber, DefaultCommissionPercent));

        await agencyRepo.AddAsync(agency, token);
        await db.SaveChangesAsync(token);
        return id;
    }

    public async Task<bool> Handle(ApproveAgencyCommand command, CancellationToken token)
    {
        var agency = await GetAgencyAsync(command.AgencyId, token);
        agency.Approve();
        return await SaveAsync(agency, token);
    }

    public async Task<bool> Handle(RejectAgencyCommand command, CancellationToken token)
    {
        var agency = await GetAgencyAsync(command.AgencyId, token);
        agency.Reject(command.Reason);
        return await SaveAsync(agency, token);
    }

    public async Task<bool> Handle(SuspendAgencyCommand command, CancellationToken token)
    {
        var agency = await GetAgencyAsync(command.AgencyId, token);
        agency.Suspend(command.Reason);
        return await SaveAsync(agency, token);
    }

    public async Task<bool> Handle(RenewAgencyLicenseCommand command, CancellationToken token)
    {
        var agency = await GetAgencyAsync(command.AgencyId, token);
        agency.RenewLicense(command.LicenseNumber, command.LicenseExpiryDate);
        return await SaveAsync(agency, token);
    }

    public async Task<bool> Handle(UpdateAgencyProfileCommand command, CancellationToken token)
    {
        var agency = await GetAgencyAsync(command.AgencyId, token);
        agency.UpdateProfile(command.Name, command.City, command.District, command.Address, command.PhoneNumber);
        return await SaveAsync(agency, token);
    }

    public async Task<bool> Handle(SetAgencyCommissionCommand command, CancellationToken token)
    {
        var agency = await GetAgencyAsync(command.AgencyId, token);
        agency.SetCommission(command.CommissionPercent);
        return await SaveAsync(agency, token);
    }

    public async Task<bool> Handle(SetAgencyServicePriceCommand command, CancellationToken token)
    {
        var agency  = await GetAgencyAsync(command.AgencyId, token);
        var priceId = await agencyRepo.GetNextPriceIdAsync();
        agency.SetServicePrice(priceId, command.ServiceType, command.ShiftKind,
                               command.Price, command.CaregiverPayRate);
        return await SaveAsync(agency, token);
    }

    private async Task<CareAgency> GetAgencyAsync(long id, CancellationToken ct)
        => await agencyRepo.GetByIdAsync(id, ct) ?? throw new AgencyNotFoundException();

    private async Task<bool> SaveAsync(CareAgency agency, CancellationToken ct)
    {
        await agencyRepo.UpdateAsync(agency, ct);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
