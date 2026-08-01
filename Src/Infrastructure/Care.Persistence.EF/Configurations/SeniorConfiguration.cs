using Care.Domain.Senior.Aggregates;
using Care.Domain.Senior.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Care.Persistence.EF.Configurations;

public sealed class SeniorConfiguration : IEntityTypeConfiguration<SeniorProfile>
{
    public void Configure(EntityTypeBuilder<SeniorProfile> builder)
    {
        builder.ToTable("Seniors");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.FullName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NationalId).HasMaxLength(11);
        builder.Property(x => x.BirthDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.Gender).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(x => x.DependencyLevel).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.City).IsRequired().HasMaxLength(50);
        builder.Property(x => x.District).HasMaxLength(50);
        builder.Property(x => x.Address).IsRequired().HasMaxLength(300);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.Ignore(x => x.Age);
        builder.Ignore(x => x.ActiveMedications);

        builder.HasIndex(x => x.PrimaryFamilyUserId);
        builder.HasIndex(x => new { x.City, x.IsActive });

        builder.HasMany(x => x.Conditions)       .WithOne().HasForeignKey(c => c.SeniorId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Medications)      .WithOne().HasForeignKey(m => m.SeniorId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Allergies)        .WithOne().HasForeignKey(a => a.SeniorId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.EmergencyContacts).WithOne().HasForeignKey(e => e.SeniorId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.FamilyMembers)    .WithOne().HasForeignKey(f => f.SeniorId).OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Conditions)       .HasField("_conditions")       .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Medications)      .HasField("_medications")      .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Allergies)        .HasField("_allergies")        .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.EmergencyContacts).HasField("_emergencyContacts").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.FamilyMembers)    .HasField("_familyMembers")    .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class MedicalConditionConfiguration : IEntityTypeConfiguration<MedicalCondition>
{
    public void Configure(EntityTypeBuilder<MedicalCondition> builder)
    {
        builder.ToTable("MedicalConditions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Title).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.DiagnosedAt).HasColumnType("date");
        builder.HasIndex(x => x.SeniorId);
    }
}

public sealed class MedicationConfiguration : IEntityTypeConfiguration<Medication>
{
    public void Configure(EntityTypeBuilder<Medication> builder)
    {
        builder.ToTable("Medications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Dosage).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Frequency).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.Instruction).HasMaxLength(500);
        builder.Property(x => x.StartDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.EndDate).HasColumnType("date");
        builder.HasIndex(x => new { x.SeniorId, x.IsActive });
    }
}

public sealed class AllergyConfiguration : IEntityTypeConfiguration<Allergy>
{
    public void Configure(EntityTypeBuilder<Allergy> builder)
    {
        builder.ToTable("Allergies");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Title).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Reaction).HasMaxLength(500);
        builder.HasIndex(x => x.SeniorId);
    }
}

public sealed class EmergencyContactConfiguration : IEntityTypeConfiguration<EmergencyContact>
{
    public void Configure(EntityTypeBuilder<EmergencyContact> builder)
    {
        builder.ToTable("EmergencyContacts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.FullName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(15);
        builder.Property(x => x.Relation).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => new { x.SeniorId, x.Priority });
    }
}

public sealed class FamilyMemberConfiguration : IEntityTypeConfiguration<FamilyMember>
{
    public void Configure(EntityTypeBuilder<FamilyMember> builder)
    {
        builder.ToTable("FamilyMembers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Relation).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.AccessLevel).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.TimeZoneId).HasMaxLength(60);
        builder.HasIndex(x => new { x.SeniorId, x.UserId }).IsUnique();
        builder.HasIndex(x => x.UserId);
    }
}
