using Care.Domain.Caregiver.Enums;
using Care.Domain.Common.Enums;

namespace Care.Domain.Caregiver.Args;

public sealed record CreateCaregiverArg(
    long           Id,
    long           UserId,
    long           AgencyId,
    string         FullName,
    string         NationalId,
    Gender         Gender,
    int            BirthYear,
    CaregiverLevel Level);
