using Care.Domain.Common.Enums;
using Care.Domain.Senior.Enums;

namespace Care.Domain.Senior.Args;

public sealed record CreateSeniorArg(
    long            Id,
    long            PrimaryMemberId,
    string          FullName,
    string?         NationalId,
    DateOnly        BirthDate,
    Gender          Gender,
    DependencyLevel DependencyLevel,
    string          City,
    string?         District,
    string          Address,
    double?         Latitude,
    double?         Longitude,
    long            PrimaryFamilyUserId,
    string?         TimeZoneId,
    string?         Notes);
