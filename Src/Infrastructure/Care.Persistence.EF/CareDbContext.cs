using Care.Domain.Agency.Aggregates;
using Care.Domain.Agency.Entities;
using Care.Domain.Billing.Aggregates;
using Care.Domain.Caregiver.Aggregates;
using Care.Domain.Caregiver.Entities;
using Care.Domain.Identity.Aggregates;
using Care.Domain.Identity.Entities;
using Care.Domain.Report.Aggregates;
using Care.Domain.Report.Entities;
using Care.Domain.Senior.Aggregates;
using Care.Domain.Senior.Entities;
using Care.Domain.Shift.Aggregates;
using OtpRequest = Care.Domain.Identity.Entities.OtpRequest;
using RefreshToken = Care.Domain.Identity.Entities.RefreshToken;

namespace Care.Persistence.EF;

/// <summary>DbContext اصلی پلتفرم مراقبت از سالمندان</summary>
public sealed class CareDbContext(DbContextOptions<CareDbContext> options) : DbContext(options)
{
    // ── Identity ──────────────────────────────────────────────────────────
    public DbSet<User>         Users         { get; set; }
    public DbSet<OtpRequest>   OtpRequests   { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    // ── Agency (مؤسسه پرستاری) ────────────────────────────────────────────
    public DbSet<CareAgency>         Agencies             { get; set; }
    public DbSet<AgencyServicePrice> AgencyServicePrices  { get; set; }

    // ── Caregiver (پرستار) ────────────────────────────────────────────────
    public DbSet<CaregiverProfile> Caregivers      { get; set; }
    public DbSet<CaregiverSkill>   CaregiverSkills { get; set; }

    // ── Senior (پرونده مراقبت) ────────────────────────────────────────────
    public DbSet<SeniorProfile>    Seniors           { get; set; }
    public DbSet<MedicalCondition> MedicalConditions { get; set; }
    public DbSet<Medication>       Medications       { get; set; }
    public DbSet<Allergy>          Allergies         { get; set; }
    public DbSet<EmergencyContact> EmergencyContacts { get; set; }
    public DbSet<FamilyMember>     FamilyMembers     { get; set; }

    // ── Shift (شیفت مراقبت) ───────────────────────────────────────────────
    public DbSet<CareShift> Shifts { get; set; }

    // ── Report (گزارش شیفت) ───────────────────────────────────────────────
    public DbSet<ShiftReport>      ShiftReports       { get; set; }
    public DbSet<MedicationIntake> MedicationIntakes  { get; set; }

    // ── Billing (پرداخت امانی) ────────────────────────────────────────────
    public DbSet<CareOrder> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CareDbContext).Assembly);
    }
}
