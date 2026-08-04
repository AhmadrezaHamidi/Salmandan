using Care.Domain.Billing.Aggregates;
using Care.Domain.Report.Aggregates;
using Care.Domain.Report.Entities;
using Care.Domain.Shift.Aggregates;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Care.Persistence.EF.Configurations;

public sealed class ShiftConfiguration : IEntityTypeConfiguration<CareShift>
{
    public void Configure(EntityTypeBuilder<CareShift> builder)
    {
        builder.ToTable("Shifts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.ServiceType).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.ShiftKind).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.CancelledBy).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Date).HasColumnType("date").IsRequired();
        builder.Property(x => x.StartTime).HasColumnType("time").IsRequired();
        builder.Property(x => x.EndTime).HasColumnType("time").IsRequired();
        builder.Property(x => x.Price).HasPrecision(18, 0);
        builder.Property(x => x.CaregiverPayRate).HasPrecision(18, 0);
        builder.Property(x => x.CancelReason).HasMaxLength(500);
        builder.Property(x => x.FamilyNote).HasMaxLength(1000);

        builder.Ignore(x => x.DurationHours);
        builder.Ignore(x => x.StartsAt);
        builder.Ignore(x => x.IsCancellable);
        builder.Ignore(x => x.Margin);
        builder.Ignore(x => x.WorkedHours);
        builder.Ignore(x => x.IsPaidOut);

        // پایه‌ی کوئری حقوق: شیفت تکمیل‌شده‌ی هنوز پرداخت‌نشده
        builder.HasIndex(x => new { x.AgencyId, x.Status, x.PayoutId });

        builder.HasIndex(x => new { x.SeniorId, x.Date });
        builder.HasIndex(x => new { x.AgencyId, x.Status });
        builder.HasIndex(x => new { x.CaregiverId, x.Date });
        builder.HasIndex(x => new { x.Status, x.Date });

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class ShiftReportConfiguration : IEntityTypeConfiguration<ShiftReport>
{
    public void Configure(EntityTypeBuilder<ShiftReport> builder)
    {
        builder.ToTable("ShiftReports");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Mood).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(x => x.Appetite).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(x => x.SleepQuality).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(x => x.Mobility).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.IncidentKind).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.IncidentDescription).HasMaxLength(1000);
        builder.Property(x => x.Temperature).HasPrecision(4, 1);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.Ignore(x => x.RiskScore);
        builder.Ignore(x => x.MedicationAdherence);

        builder.HasIndex(x => x.ShiftId).IsUnique();
        builder.HasIndex(x => new { x.SeniorId, x.SubmittedAt });

        builder.HasMany(x => x.MedicationIntakes)
               .WithOne()
               .HasForeignKey(m => m.ReportId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.MedicationIntakes).HasField("_medicationIntakes").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class MedicationIntakeConfiguration : IEntityTypeConfiguration<MedicationIntake>
{
    public void Configure(EntityTypeBuilder<MedicationIntake> builder)
    {
        builder.ToTable("MedicationIntakes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.MedicationName).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.HasIndex(x => new { x.ReportId, x.MedicationId }).IsUnique();
    }
}

public sealed class CareOrderConfiguration : IEntityTypeConfiguration<CareOrder>
{
    public void Configure(EntityTypeBuilder<CareOrder> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Amount).HasPrecision(18, 0);
        builder.Property(x => x.PlatformFee).HasPrecision(18, 0);
        builder.Property(x => x.AgencyShare).HasPrecision(18, 0);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.PaymentRefId).HasMaxLength(100);
        builder.Property(x => x.PaymentAuthority).HasMaxLength(100);
        builder.Property(x => x.RefundReason).HasMaxLength(500);
        builder.Property(x => x.InvoiceNumber).HasMaxLength(50);

        builder.HasIndex(x => x.ShiftId).IsUnique();
        builder.HasIndex(x => x.PaymentAuthority);
        builder.HasIndex(x => new { x.SeniorId, x.Status });

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
