using Care.Domain.Agency.Exceptions;
using Care.Domain.Caregiver.Aggregates;
using Care.Domain.Caregiver.Args;
using Care.Domain.Caregiver.Exceptions;
using Care.Domain.Identity.Aggregates;
using Care.Domain.Identity.Args;
using Care.Domain.Identity.Enums;

namespace Care.Application.Caregiver;

/// <summary>Handler های مدیریت پرستاران و احراز صلاحیت آن‌ها</summary>
public sealed class CaregiverHandlers(
    ICaregiverRepository caregiverRepo,
    IAgencyRepository    agencyRepo,
    IUserRepository      userRepo,
    CareDbContext        db) :
    ICommandHandler<RegisterCaregiverCommand, long>,
    ICommandHandler<SubmitCaregiverDocumentsCommand, bool>,
    ICommandHandler<VerifyCaregiverCommand, bool>,
    ICommandHandler<RejectCaregiverCommand, bool>,
    ICommandHandler<SuspendCaregiverCommand, bool>,
    ICommandHandler<UpdateCaregiverProfileCommand, bool>,
    ICommandHandler<AddCaregiverSkillCommand, bool>,
    ICommandHandler<RemoveCaregiverSkillCommand, bool>
{
    public async Task<long> Handle(RegisterCaregiverCommand command, CancellationToken token)
    {
        var agency = await agencyRepo.GetByIdAsync(command.AgencyId, token)
            ?? throw new AgencyNotFoundException();

        if (!agency.CanAcceptShifts)
            throw new AgencyNotActiveException();

        if (await caregiverRepo.NationalIdExistsAsync(command.NationalId, token))
            throw new DuplicateNationalIdException();

        // حساب کاربری پرستار در صورت نبود ساخته می‌شود تا بتواند وارد اپ پرستار شود.
        // اگر شماره از قبل ثبت‌نام کرده باشد، نقشش به پرستار ارتقا می‌یابد —
        // بدون این، کسی که قبلاً به‌عنوان خانواده وارد شده هرگز نقش پرستار نمی‌گرفت.
        var user = await userRepo.GetByPhoneAsync(command.PhoneNumber, token);
        if (user is null)
        {
            var userId = await userRepo.GetNextIdAsync();
            user = User.Create(new CreateUserArg(userId, command.PhoneNumber, UserRole.Caregiver));
            await userRepo.AddAsync(user, token);
        }
        else
        {
            user.PromoteTo(UserRole.Caregiver);
            await userRepo.UpdateAsync(user, token);
        }

        var id        = await caregiverRepo.GetNextIdAsync();
        var caregiver = CaregiverProfile.Register(new CreateCaregiverArg(
            id, user.Id, command.AgencyId, command.FullName, command.NationalId,
            command.Gender, command.BirthYear, command.Level));

        await caregiverRepo.AddAsync(caregiver, token);
        await db.SaveChangesAsync(token);
        return id;
    }

    public async Task<bool> Handle(SubmitCaregiverDocumentsCommand command, CancellationToken token)
    {
        var caregiver = await GetCaregiverAsync(command.CaregiverId, token);
        caregiver.SubmitDocuments(command.HasCriminalRecordCheck, command.HasCertificate, command.HasLiabilityInsurance);
        return await SaveAsync(caregiver, token);
    }

    public async Task<bool> Handle(VerifyCaregiverCommand command, CancellationToken token)
    {
        var caregiver = await GetCaregiverAsync(command.CaregiverId, token);
        caregiver.Verify();
        return await SaveAsync(caregiver, token);
    }

    public async Task<bool> Handle(RejectCaregiverCommand command, CancellationToken token)
    {
        var caregiver = await GetCaregiverAsync(command.CaregiverId, token);
        caregiver.Reject(command.Reason);
        return await SaveAsync(caregiver, token);
    }

    public async Task<bool> Handle(SuspendCaregiverCommand command, CancellationToken token)
    {
        var caregiver = await GetCaregiverAsync(command.CaregiverId, token);
        caregiver.Suspend(command.Reason);
        return await SaveAsync(caregiver, token);
    }

    public async Task<bool> Handle(UpdateCaregiverProfileCommand command, CancellationToken token)
    {
        var caregiver = await GetCaregiverAsync(command.CaregiverId, token);
        caregiver.UpdateProfile(command.FullName, command.Level);
        return await SaveAsync(caregiver, token);
    }

    public async Task<bool> Handle(AddCaregiverSkillCommand command, CancellationToken token)
    {
        var caregiver = await GetCaregiverAsync(command.CaregiverId, token);
        var skillId   = await caregiverRepo.GetNextSkillIdAsync();
        caregiver.AddSkill(skillId, command.SkillType, command.YearsOfExperience);
        return await SaveAsync(caregiver, token);
    }

    public async Task<bool> Handle(RemoveCaregiverSkillCommand command, CancellationToken token)
    {
        var caregiver = await GetCaregiverAsync(command.CaregiverId, token);
        caregiver.RemoveSkill(command.SkillType);
        return await SaveAsync(caregiver, token);
    }

    private async Task<CaregiverProfile> GetCaregiverAsync(long id, CancellationToken ct)
        => await caregiverRepo.GetByIdAsync(id, ct) ?? throw new CaregiverNotFoundException();

    private async Task<bool> SaveAsync(CaregiverProfile caregiver, CancellationToken ct)
    {
        await caregiverRepo.UpdateAsync(caregiver, ct);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
