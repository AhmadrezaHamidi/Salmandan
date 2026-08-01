using Care.Domain.Agency.Aggregates;
using Care.Domain.Agency.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Care.Persistence.EF.Configurations;

public sealed class AgencyConfiguration : IEntityTypeConfiguration<CareAgency>
{
    public void Configure(EntityTypeBuilder<CareAgency> builder)
    {
        builder.ToTable("Agencies");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
        builder.Property(x => x.LicenseNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.LicenseExpiryDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.City).IsRequired().HasMaxLength(50);
        builder.Property(x => x.District).HasMaxLength(50);
        builder.Property(x => x.Address).HasMaxLength(300);
        builder.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(15);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.CommissionPercent).HasPrecision(5, 2);
        builder.Property(x => x.RatingAvg).HasPrecision(3, 2);
        builder.Property(x => x.RejectionReason).HasMaxLength(500);

        builder.Ignore(x => x.HasValidLicense);
        builder.Ignore(x => x.CanAcceptShifts);

        builder.HasIndex(x => x.LicenseNumber).IsUnique();
        builder.HasIndex(x => new { x.City, x.Status });
        builder.HasIndex(x => x.OwnerUserId);

        builder.HasMany(x => x.Prices)
               .WithOne()
               .HasForeignKey(p => p.AgencyId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Prices).HasField("_prices").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class AgencyServicePriceConfiguration : IEntityTypeConfiguration<AgencyServicePrice>
{
    public void Configure(EntityTypeBuilder<AgencyServicePrice> builder)
    {
        builder.ToTable("AgencyServicePrices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.ServiceType).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.ShiftKind).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Price).HasPrecision(18, 0);
        builder.HasIndex(x => new { x.AgencyId, x.ServiceType, x.ShiftKind }).IsUnique();
    }
}
