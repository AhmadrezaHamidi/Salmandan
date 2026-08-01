using Care.Domain.Caregiver.Aggregates;
using Care.Domain.Caregiver.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Care.Persistence.EF.Configurations;

public sealed class CaregiverConfiguration : IEntityTypeConfiguration<CaregiverProfile>
{
    public void Configure(EntityTypeBuilder<CaregiverProfile> builder)
    {
        builder.ToTable("Caregivers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.FullName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NationalId).IsRequired().HasMaxLength(11);
        builder.Property(x => x.Gender).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(x => x.Level).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.RatingAvg).HasPrecision(3, 2);
        builder.Property(x => x.StatusReason).HasMaxLength(500);

        builder.Ignore(x => x.CanBeAssigned);

        builder.HasIndex(x => x.NationalId).IsUnique();
        builder.HasIndex(x => new { x.AgencyId, x.Status });
        builder.HasIndex(x => x.UserId);

        builder.HasMany(x => x.Skills)
               .WithOne()
               .HasForeignKey(s => s.CaregiverId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Skills).HasField("_skills").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class CaregiverSkillConfiguration : IEntityTypeConfiguration<CaregiverSkill>
{
    public void Configure(EntityTypeBuilder<CaregiverSkill> builder)
    {
        builder.ToTable("CaregiverSkills");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.SkillType).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.HasIndex(x => new { x.CaregiverId, x.SkillType }).IsUnique();
    }
}
