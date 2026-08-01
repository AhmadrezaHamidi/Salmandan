using Care.Domain.Identity.Aggregates;
using Care.Domain.Identity.Args;
using Care.Domain.Identity.Enums;
using Care.Domain.Identity.Exceptions;
using Care.Domain.Senior.Aggregates;
using Care.Domain.Senior.Args;
using Care.Domain.Senior.Exceptions;

namespace Care.Application.Senior;

/// <summary>
/// Handler های پرونده مراقبت سالمند و حلقه خانواده.
/// هر عملیات ابتدا دسترسی کاربر به پرونده را بررسی می‌کند — داده سلامت است.
/// </summary>
public sealed class SeniorHandlers(
    ISeniorRepository seniorRepo,
    IUserRepository   userRepo,
    CareDbContext     db) :
    ICommandHandler<CreateSeniorCommand, long>,
    ICommandHandler<UpdateSeniorCommand, bool>,
    ICommandHandler<ChangeDependencyLevelCommand, bool>,
    ICommandHandler<AddFamilyMemberCommand, bool>,
    ICommandHandler<RemoveFamilyMemberCommand, bool>,
    ICommandHandler<ChangeFamilyMemberAccessCommand, bool>,
    ICommandHandler<AddConditionCommand, bool>,
    ICommandHandler<RemoveConditionCommand, bool>,
    ICommandHandler<AddMedicationCommand, bool>,
    ICommandHandler<DiscontinueMedicationCommand, bool>,
    ICommandHandler<AddAllergyCommand, bool>,
    ICommandHandler<AddEmergencyContactCommand, bool>
{
    public async Task<long> Handle(CreateSeniorCommand command, CancellationToken token)
    {
        _ = await userRepo.GetByIdAsync(command.CreatedByUserId, token)
            ?? throw new UserNotFoundException();

        var id       = await seniorRepo.GetNextIdAsync();
        var memberId = await seniorRepo.GetNextFamilyMemberIdAsync();

        var senior = SeniorProfile.Create(new CreateSeniorArg(
            id, memberId, command.FullName, command.NationalId, command.BirthDate,
            command.Gender, command.DependencyLevel, command.City, command.District,
            command.Address, command.Latitude, command.Longitude,
            command.CreatedByUserId, command.TimeZoneId, command.Notes));

        await seniorRepo.AddAsync(senior, token);
        await db.SaveChangesAsync(token);
        return id;
    }

    public async Task<bool> Handle(UpdateSeniorCommand command, CancellationToken token)
    {
        var senior = await GetWritableAsync(command.SeniorId, command.ActorUserId, token);
        senior.UpdateProfile(command.FullName, command.City, command.District, command.Address,
                             command.Latitude, command.Longitude, command.Notes);
        return await SaveAsync(senior, token);
    }

    public async Task<bool> Handle(ChangeDependencyLevelCommand command, CancellationToken token)
    {
        var senior = await GetWritableAsync(command.SeniorId, command.ActorUserId, token);
        senior.ChangeDependencyLevel(command.DependencyLevel);
        return await SaveAsync(senior, token);
    }

    // ── حلقه خانواده ──────────────────────────────────────────────────────

    public async Task<bool> Handle(AddFamilyMemberCommand command, CancellationToken token)
    {
        var senior = await GetManagedAsync(command.SeniorId, command.ActorUserId, token);

        // اگر کاربر هنوز ثبت‌نام نکرده، حسابش ساخته می‌شود تا با OTP وارد شود
        var user = await userRepo.GetByPhoneAsync(command.PhoneNumber, token);
        if (user is null)
        {
            var userId = await userRepo.GetNextIdAsync();
            user = User.Create(new CreateUserArg(userId, command.PhoneNumber, UserRole.Family));
            await userRepo.AddAsync(user, token);
            await db.SaveChangesAsync(token);
        }

        var memberId = await seniorRepo.GetNextFamilyMemberIdAsync();
        senior.AddFamilyMember(memberId, user.Id, command.Relation, command.AccessLevel, command.TimeZoneId);
        return await SaveAsync(senior, token);
    }

    public async Task<bool> Handle(RemoveFamilyMemberCommand command, CancellationToken token)
    {
        var senior = await GetManagedAsync(command.SeniorId, command.ActorUserId, token);
        senior.RemoveFamilyMember(command.MemberUserId);
        return await SaveAsync(senior, token);
    }

    public async Task<bool> Handle(ChangeFamilyMemberAccessCommand command, CancellationToken token)
    {
        var senior = await GetManagedAsync(command.SeniorId, command.ActorUserId, token);
        senior.ChangeFamilyMemberAccess(command.MemberUserId, command.AccessLevel);
        return await SaveAsync(senior, token);
    }

    // ── پرونده پزشکی ──────────────────────────────────────────────────────

    public async Task<bool> Handle(AddConditionCommand command, CancellationToken token)
    {
        var senior      = await GetWritableAsync(command.SeniorId, command.ActorUserId, token, full: true);
        var conditionId = await seniorRepo.GetNextConditionIdAsync();
        senior.AddCondition(conditionId, command.Title, command.Description, command.DiagnosedAt, command.IsChronic);
        return await SaveAsync(senior, token);
    }

    public async Task<bool> Handle(RemoveConditionCommand command, CancellationToken token)
    {
        var senior = await GetWritableAsync(command.SeniorId, command.ActorUserId, token, full: true);
        senior.RemoveCondition(command.ConditionId);
        return await SaveAsync(senior, token);
    }

    public async Task<bool> Handle(AddMedicationCommand command, CancellationToken token)
    {
        var senior       = await GetWritableAsync(command.SeniorId, command.ActorUserId, token, full: true);
        var medicationId = await seniorRepo.GetNextMedicationIdAsync();
        senior.AddMedication(medicationId, command.Name, command.Dosage, command.Frequency,
                             command.Instruction, command.StartDate, command.EndDate);
        return await SaveAsync(senior, token);
    }

    public async Task<bool> Handle(DiscontinueMedicationCommand command, CancellationToken token)
    {
        var senior = await GetWritableAsync(command.SeniorId, command.ActorUserId, token, full: true);
        senior.DiscontinueMedication(command.MedicationId);
        return await SaveAsync(senior, token);
    }

    public async Task<bool> Handle(AddAllergyCommand command, CancellationToken token)
    {
        var senior    = await GetWritableAsync(command.SeniorId, command.ActorUserId, token, full: true);
        var allergyId = await seniorRepo.GetNextAllergyIdAsync();
        senior.AddAllergy(allergyId, command.Title, command.Reaction);
        return await SaveAsync(senior, token);
    }

    public async Task<bool> Handle(AddEmergencyContactCommand command, CancellationToken token)
    {
        var senior    = await GetWritableAsync(command.SeniorId, command.ActorUserId, token, full: true);
        var contactId = await seniorRepo.GetNextContactIdAsync();
        senior.AddEmergencyContact(contactId, command.FullName, command.PhoneNumber, command.Relation, command.Priority);
        return await SaveAsync(senior, token);
    }

    // ── کنترل دسترسی ──────────────────────────────────────────────────────

    /// <summary>پرونده‌ای که کاربر حق ویرایش دارد (Manager یا Contributor)</summary>
    private async Task<SeniorProfile> GetWritableAsync(long seniorId, long actorUserId, CancellationToken ct, bool full = false)
    {
        var senior = full
            ? await seniorRepo.GetFullAsync(seniorId, ct)
            : await seniorRepo.GetWithFamilyAsync(seniorId, ct);

        if (senior is null)                   throw new SeniorNotFoundException();
        if (!senior.CanBookShift(actorUserId)) throw new AccessDeniedException();
        return senior;
    }

    /// <summary>پرونده‌ای که کاربر مدیر آن است</summary>
    private async Task<SeniorProfile> GetManagedAsync(long seniorId, long actorUserId, CancellationToken ct)
    {
        var senior = await seniorRepo.GetWithFamilyAsync(seniorId, ct)
            ?? throw new SeniorNotFoundException();

        if (!senior.IsManager(actorUserId)) throw new AccessDeniedException();
        return senior;
    }

    private async Task<bool> SaveAsync(SeniorProfile senior, CancellationToken ct)
    {
        await seniorRepo.UpdateAsync(senior, ct);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
