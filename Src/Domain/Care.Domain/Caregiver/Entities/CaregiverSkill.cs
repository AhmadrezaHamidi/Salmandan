using Care.Domain.Caregiver.Enums;

namespace Care.Domain.Caregiver.Entities;

/// <summary>مهارت تخصصی پرستار</summary>
public sealed class CaregiverSkill
{
    public long               Id                { get; private set; }
    public long               CaregiverId       { get; private set; }
    public CaregiverSkillType SkillType         { get; private set; }
    public int                YearsOfExperience { get; private set; }

    private CaregiverSkill() { }

    public CaregiverSkill(long id, long caregiverId, CaregiverSkillType skillType, int yearsOfExperience)
    {
        Id                = id;
        CaregiverId       = caregiverId;
        SkillType         = skillType;
        YearsOfExperience = yearsOfExperience;
    }
}
