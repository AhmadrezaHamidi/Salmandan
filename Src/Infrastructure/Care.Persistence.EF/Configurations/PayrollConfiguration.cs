using Care.Domain.Payroll.Aggregates;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Care.Persistence.EF.Configurations;

public sealed class PayoutConfiguration : IEntityTypeConfiguration<CaregiverPayout>
{
    public void Configure(EntityTypeBuilder<CaregiverPayout> builder)
    {
        builder.ToTable("Payouts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.PeriodFrom).HasColumnType("date").IsRequired();
        builder.Property(x => x.PeriodTo).HasColumnType("date").IsRequired();
        builder.Property(x => x.TotalHours).HasPrecision(9, 2);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 0);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.PaymentReference).HasMaxLength(100);
        builder.Property(x => x.CancelReason).HasMaxLength(500);

        builder.Ignore(x => x.HourlyRate);

        builder.HasIndex(x => new { x.AgencyId, x.Status });
        builder.HasIndex(x => new { x.CaregiverId, x.PeriodFrom });

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
